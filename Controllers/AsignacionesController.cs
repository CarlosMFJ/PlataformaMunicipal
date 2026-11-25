using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
    public class AsignacionesController : AppControllerBase
    {
        public AsignacionesController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.AsignacionSolicituds
                .Include(a => a.Solicitud).Include(a => a.Departamento)
                .Include(a => a.Empleado).Include(a => a.AsignadoPorUsuario)
                .OrderByDescending(a => a.FechaAsignacion).ToListAsync());

        public IActionResult Crear()
        {
            CargarListas();
            return PartialView("_Form", new AsignacionSolicitud { EsActual = true });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(AsignacionSolicitud m)
        {
            LimpiarNavegacion<AsignacionSolicitud>();
            ModelState.Remove("AsignadoPorUsuarioId");
            if (!ModelState.IsValid) { CargarListas(m); return PartialView("_Form", m); }

            using var tx = await _db.Database.BeginTransactionAsync();
            if (m.EsActual)
            {
                var previas = await _db.AsignacionSolicituds
                    .Where(a => a.SolicitudId == m.SolicitudId && a.EsActual).ToListAsync();
                foreach (var a in previas) a.EsActual = false;
                await _db.SaveChangesAsync();
            }
            m.AsignadoPorUsuarioId = UsuarioActualId;
            m.FechaAsignacion = DateTime.Now;
            _db.Add(m);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Json(new { ok = true });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.AsignacionSolicituds.FindAsync(id);
            if (m == null) return NotFound();
            CargarListas(m);
            return PartialView("_Form", m);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(AsignacionSolicitud m)
        {
            LimpiarNavegacion<AsignacionSolicitud>();
            ModelState.Remove("AsignadoPorUsuarioId");
            if (!ModelState.IsValid) { CargarListas(m); return PartialView("_Form", m); }

            var a = await _db.AsignacionSolicituds.FindAsync(m.AsignacionId);
            if (a == null) return NotFound();

            a.DepartamentoId = m.DepartamentoId;
            a.EmpleadoId = m.EmpleadoId;
            a.FechaLimite = m.FechaLimite;
            a.FechaCompletado = m.FechaCompletado;
            a.Comentario = m.Comentario;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var a = await _db.AsignacionSolicituds.FindAsync(id);
            if (a == null) return NotFound();
            _db.AsignacionSolicituds.Remove(a);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private void CargarListas(AsignacionSolicitud m = null)
        {
            ViewBag.Solicitudes = new SelectList(
                _db.SolicitudCiudadanas.OrderByDescending(s => s.FechaSolicitud)
                    .Select(s => new { s.SolicitudId, Nombre = s.NumeroSolicitud + " - " + s.Asunto }),
                "SolicitudId", "Nombre", m?.SolicitudId);
            ViewBag.Departamentos = new SelectList(
                _db.Departamentos.Where(d => d.Activo).OrderBy(d => d.Nombre),
                "DepartamentoId", "Nombre", m?.DepartamentoId);
            ViewBag.Empleados = new SelectList(
                _db.Empleados.Where(e => e.Activo)
                    .Select(e => new { e.EmpleadoId, Nombre = e.Nombres + " " + e.Apellidos }),
                "EmpleadoId", "Nombre", m?.EmpleadoId);
        }
    }
}
