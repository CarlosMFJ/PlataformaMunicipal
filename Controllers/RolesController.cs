using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolesController : AppControllerBase
    {
        public RolesController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.Rols.OrderBy(r => r.NivelAcceso).ToListAsync());

        public IActionResult Crear() => PartialView("_Form", new Rol { Activo = true, NivelAcceso = 4 });

        [HttpPost]
        public async Task<IActionResult> Crear(Rol m)
        {
            LimpiarNavegacion<Rol>();
            if (await _db.Rols.AnyAsync(r => r.Nombre == m.Nombre))
                ModelState.AddModelError("Nombre", "Ya existe un rol con ese nombre");
            if (!ModelState.IsValid) return PartialView("_Form", m);

            m.FechaCreacion = DateTime.Now;
            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.Rols.FindAsync(id);
            return m == null ? NotFound() : PartialView("_Form", m);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Rol m)
        {
            LimpiarNavegacion<Rol>();
            if (await _db.Rols.AnyAsync(r => r.Nombre == m.Nombre && r.RolId != m.RolId))
                ModelState.AddModelError("Nombre", "Ya existe un rol con ese nombre");
            if (!ModelState.IsValid) return PartialView("_Form", m);

            _db.Update(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var r = await _db.Rols.FindAsync(id);
            if (r == null) return NotFound();
            if (r.Nombre == "Administrador")
                return Json(new { ok = false, mensaje = "No se puede desactivar el rol Administrador." });
            r.Activo = false;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }
    }
}
