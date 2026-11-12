using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Helpers;
using PlataformaMunicipal.Models;
using PlataformaMunicipal.ViewModels;

namespace PlataformaMunicipal.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) => _db = db;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var u = await _db.Usuarios.Include(x => x.Rol)
                .FirstOrDefaultAsync(x => x.NombreUsuario == vm.Usuario && x.Activo);

            if (u != null && u.Bloqueado)
            {
                ModelState.AddModelError("", "Cuenta bloqueada. Contacte al administrador.");
                return View(vm);
            }

            if (u == null || !PasswordHelper.Verify(vm.Contrasena, u.ContrasenaHash))
            {
                if (u != null)
                {
                    u.IntentosFallidos++;
                    if (u.IntentosFallidos >= 5) u.Bloqueado = true;
                    await _db.SaveChangesAsync();
                }
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                return View(vm);
            }

            u.IntentosFallidos = 0;
            u.UltimoAcceso = DateTime.Now;
            await _db.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, u.UsuarioId.ToString()),
                new(ClaimTypes.Name, u.NombreUsuario),
                new(ClaimTypes.Role, u.Rol.Nombre)
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult Denegado() => View();
    }
}