using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Helpers;
using PlataformaMunicipal.Models;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Las claves viven solo en memoria: al reiniciar la app, las cookies viejas dejan de valer
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/Denegado";
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.MaxAge = null;   // cookie de sesión: se borra al cerrar el navegador

        // En cada petición confirma que el usuario sigue activo y no bloqueado
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var idTxt = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var valido = int.TryParse(idTxt, out var id) &&
                         await db.Usuarios.AnyAsync(u => u.UsuarioId == id && u.Activo && !u.Bloqueado);

            if (!valido)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

// Toda la app exige login, salvo lo marcado con [AllowAnonymous]
builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Siembra del administrador inicial
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Usuarios.Any())
    {
        var tic = db.Departamentos.First(d => d.Codigo == "TIC");
        var emp = new Empleado
        {
            DepartamentoId = tic.DepartamentoId,
            Cedula = "00000000000",
            Nombres = "Admin",
            Apellidos = "Sistema",
            Cargo = "Administrador",
            Correo = "admin@ayuntamiento.gob.do",
            FechaIngreso = DateOnly.FromDateTime(DateTime.Today),
            Activo = true
        };
        db.Empleados.Add(emp);
        db.SaveChanges();

        db.Usuarios.Add(new Usuario
        {
            RolId = db.Rols.First(r => r.Nombre == "Administrador").RolId,
            EmpleadoId = emp.EmpleadoId,
            NombreUsuario = "admin",
            Correo = emp.Correo,
            ContrasenaHash = PasswordHelper.Hash("Admin123*"),
            Activo = true
        });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Evita que el botón "Atrás" muestre páginas después de cerrar sesión
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["Cache-Control"] = "no-store, no-cache";
    ctx.Response.Headers["Pragma"] = "no-cache";
    await next();
});

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Publico}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();