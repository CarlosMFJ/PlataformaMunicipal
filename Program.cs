using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Helpers;
using PlataformaMunicipal.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/Denegado";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();

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
            FechaIngreso = DateOnly.FromDateTime(DateTime.Today)
        };
        db.Empleados.Add(emp);
        db.SaveChanges();

        db.Usuarios.Add(new Usuario
        {
            RolId = db.Rols.First(r => r.Nombre == "Administrador").RolId,
            EmpleadoId = emp.EmpleadoId,
            NombreUsuario = "admin",
            Correo = emp.Correo,
            ContrasenaHash = PasswordHelper.Hash("Admin123*")
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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
