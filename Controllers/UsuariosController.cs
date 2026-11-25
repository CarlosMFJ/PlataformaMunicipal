using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Helpers;
using PlataformaMunicipal.Models;
using PlataformaMunicipal.ViewModels;

namespace PlataformaMunicipal.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : AppControllerBase
    {
        public UsuariosController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.Usuarios.Include(u => u.Rol)
                .Include(u => u.Empleado).Include(u => u.Ciudadano)
                .OrderBy(u => u.NombreUsuario).ToListAsync());

        public IActionResult Crear()
        {
            CargarListas();
            return PartialView("_Form", new UsuarioFormViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Crear(UsuarioFormViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Contrasena) || vm.Contrasena.Length < 8)
                ModelState.AddModelError("Contrasena", "La contraseña debe tener al menos 8 caracteres");
            await Validar(vm);
            if (!ModelState.IsValid) { CargarListas(vm); return PartialView("_Form", vm); }

            _db.Usuarios.Add(new Usuario
            {
                RolId = vm.RolId,
                EmpleadoId = vm.EmpleadoId,
                CiudadanoId = vm.CiudadanoId,
                NombreUsuario = vm.NombreUsuario,
                Correo = vm.Correo,
                ContrasenaHash = PasswordHelper.Hash(vm.Contrasena),
                Activo = vm.Activo,
                Bloqueado = false,
                FechaCreacion = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var u = await _db.Usuarios.FindAsync(id);
            if (u == null) return NotFound();
            var vm = new UsuarioFormViewModel
            {
                UsuarioId = u.UsuarioId,
                RolId = u.RolId,
                EmpleadoId = u.EmpleadoId,
                CiudadanoId = u.CiudadanoId,
                NombreUsuario = u.NombreUsuario,
                Correo = u.Correo,
                Activo = u.Activo,
                Bloqueado = u.Bloqueado
            };
            CargarListas(vm);
            return PartialView("_Form", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(UsuarioFormViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.Contrasena) && vm.Contrasena.Length < 8)
                ModelState.AddModelError("Contrasena", "La contraseña debe tener al menos 8 caracteres");
            await Validar(vm);
            if (!ModelState.IsValid) { CargarListas(vm); return PartialView("_Form", vm); }

            var u = await _db.Usuarios.FindAsync(vm.UsuarioId);
            if (u == null) return NotFound();

            u.RolId = vm.RolId;
            u.EmpleadoId = vm.EmpleadoId;
            u.CiudadanoId = vm.CiudadanoId;
            u.NombreUsuario = vm.NombreUsuario;
            u.Correo = vm.Correo;
            u.Activo = vm.Activo;
            u.Bloqueado = vm.Bloqueado;
            if (!vm.Bloqueado) u.IntentosFallidos = 0;
            if (!string.IsNullOrEmpty(vm.Contrasena))
                u.ContrasenaHash = PasswordHelper.Hash(vm.Contrasena);

            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id == UsuarioActualId)
                return Json(new { ok = false, mensaje = "No puede desactivar su propia cuenta." });
            var u = await _db.Usuarios.FindAsync(id);
            if (u == null) return NotFound();
            u.Activo = false;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private async Task Validar(UsuarioFormViewModel vm)
        {
            if ((vm.EmpleadoId == null) == (vm.CiudadanoId == null))
                ModelState.AddModelError("", "Seleccione un empleado O un ciudadano (solo uno).");
            if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == vm.NombreUsuario && u.UsuarioId != vm.UsuarioId))
                ModelState.AddModelError("NombreUsuario", "Ese nombre de usuario ya existe");
            if (await _db.Usuarios.AnyAsync(u => u.Correo == vm.Correo && u.UsuarioId != vm.UsuarioId))
                ModelState.AddModelError("Correo", "Ese correo ya está registrado");
            if (vm.EmpleadoId != null && await _db.Usuarios.AnyAsync(u => u.EmpleadoId == vm.EmpleadoId && u.UsuarioId != vm.UsuarioId))
                ModelState.AddModelError("EmpleadoId", "Ese empleado ya tiene una cuenta");
            if (vm.CiudadanoId != null && await _db.Usuarios.AnyAsync(u => u.CiudadanoId == vm.CiudadanoId && u.UsuarioId != vm.UsuarioId))
                ModelState.AddModelError("CiudadanoId", "Ese ciudadano ya tiene una cuenta");
        }

        private void CargarListas(UsuarioFormViewModel vm = null)
        {
            ViewBag.Roles = new SelectList(_db.Rols.Where(r => r.Activo).OrderBy(r => r.NivelAcceso),
                "RolId", "Nombre", vm?.RolId);
            ViewBag.Empleados = new SelectList(
                _db.Empleados.Where(e => e.Activo)
                    .Select(e => new { e.EmpleadoId, Nombre = e.Nombres + " " + e.Apellidos }),
                "EmpleadoId", "Nombre", vm?.EmpleadoId);
            ViewBag.Ciudadanos = new SelectList(
                _db.Ciudadanos.Where(c => c.Activo)
                    .Select(c => new { c.CiudadanoId, Nombre = c.Nombres + " " + c.Apellidos + " (" + c.Cedula + ")" }),
                "CiudadanoId", "Nombre", vm?.CiudadanoId);
        }
    }
}