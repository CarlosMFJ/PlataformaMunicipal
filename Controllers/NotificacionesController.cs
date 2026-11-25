using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize]
    public class NotificacionesController : AppControllerBase
    {
        public NotificacionesController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index()
        {
            var q = _db.Notificacions.Include(n => n.Usuario).Include(n => n.Solicitud).AsQueryable();
            if (!User.IsInRole("Administrador"))
                q = q.Where(n => n.UsuarioId == UsuarioActualId);
            return View(await q.OrderByDescending(n => n.FechaEnvio).ToListAsync());
        }

        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public IActionResult Crear()
        {
            CargarListas();
            return PartialView("_Form", new Notificacion { Tipo = "Informativa", Canal = "Sistema" });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Crear(Notificacion m)
        {
            LimpiarNavegacion<Notificacion>();
            if (!ModelState.IsValid) { CargarListas(m); return PartialView("_Form", m); }

            m.FechaEnvio = DateTime.Now;
            m.Leida = false;
            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.Notificacions.FindAsync(id);
            if (m == null) return NotFound();
            CargarListas(m);
            return PartialView("_Form", m);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Editar(Notificacion m)
        {
            LimpiarNavegacion<Notificacion>();
            if (!ModelState.IsValid) { CargarListas(m); return PartialView("_Form", m); }

            _db.Update(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // Marcar como leída (la usa cualquier usuario con sus propias notificaciones)
        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var n = await _db.Notificacions.FindAsync(id);
            if (n == null) return NotFound();
            if (n.UsuarioId != UsuarioActualId && !User.IsInRole("Administrador")) return Forbid();
            n.Leida = true;
            n.FechaLectura = DateTime.Now;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var n = await _db.Notificacions.FindAsync(id);
            if (n == null) return NotFound();
            _db.Notificacions.Remove(n);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private void CargarListas(Notificacion m = null)
        {
            ViewBag.Usuarios = new SelectList(_db.Usuarios.Where(u => u.Activo).OrderBy(u => u.NombreUsuario),
                "UsuarioId", "NombreUsuario", m?.UsuarioId);
            ViewBag.Solicitudes = new SelectList(
                _db.SolicitudCiudadanas.OrderByDescending(s => s.FechaSolicitud)
                    .Select(s => new { s.SolicitudId, Nombre = s.NumeroSolicitud + " - " + s.Asunto }),
                "SolicitudId", "Nombre", m?.SolicitudId);
            ViewBag.Tipos = new SelectList(new[] { "Informativa", "Alerta", "Recordatorio", "Resolucion" });
            ViewBag.Canales = new SelectList(new[] { "Sistema", "Correo", "SMS", "Push" });
        }
    }
}