using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize(Roles = "Administrador,Director")]
    public class EstadosController : AppControllerBase
    {
        public EstadosController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.EstadoSolicituds.OrderBy(e => e.Orden).ToListAsync());

        public IActionResult Crear() =>
            PartialView("_Form", new EstadoSolicitud { Activo = true, Color = "#6C757D" });

        [HttpPost]
        public async Task<IActionResult> Crear(EstadoSolicitud m)
        {
            LimpiarNavegacion<EstadoSolicitud>();
            if (await _db.EstadoSolicituds.AnyAsync(e => e.Nombre == m.Nombre))
                ModelState.AddModelError("Nombre", "Ya existe un estado con ese nombre");
            if (!ModelState.IsValid) return PartialView("_Form", m);

            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.EstadoSolicituds.FindAsync(id);
            return m == null ? NotFound() : PartialView("_Form", m);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(EstadoSolicitud m)
        {
            LimpiarNavegacion<EstadoSolicitud>();
            if (await _db.EstadoSolicituds.AnyAsync(e => e.Nombre == m.Nombre && e.EstadoId != m.EstadoId))
                ModelState.AddModelError("Nombre", "Ya existe un estado con ese nombre");
            if (!ModelState.IsValid) return PartialView("_Form", m);

            _db.Update(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var e = await _db.EstadoSolicituds.FindAsync(id);
            if (e == null) return NotFound();
            if (await _db.SolicitudCiudadanas.AnyAsync(s => s.EstadoId == id))
                return Json(new { ok = false, mensaje = "Hay solicitudes con este estado. No se puede desactivar." });
            e.Activo = false;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }
    }
}
