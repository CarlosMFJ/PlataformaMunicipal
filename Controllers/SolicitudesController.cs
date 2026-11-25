using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;
using PlataformaMunicipal.ViewModels;

namespace PlataformaMunicipal.Controllers
{
    [Authorize]
    public class SolicitudesController : AppControllerBase
    {
        public SolicitudesController(AppDbContext db) : base(db) { }

        // ---------- Listado con filtros ----------
        public async Task<IActionResult> Index(string q, int? estadoId, int? departamentoId)
        {
            var query = _db.SolicitudCiudadanas
                .Include(s => s.Ciudadano).Include(s => s.Servicio).Include(s => s.Estado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().Replace("-", "");
                query = query.Where(s => s.NumeroSolicitud.Contains(q) ||
                                         s.Ciudadano.Cedula.Contains(q) ||
                                         s.Asunto.Contains(q));
            }
            if (estadoId != null) query = query.Where(s => s.EstadoId == estadoId);
            if (departamentoId != null)
                query = query.Where(s => _db.AsignacionSolicituds
                    .Any(a => a.SolicitudId == s.SolicitudId && a.EsActual && a.DepartamentoId == departamentoId));

            ViewBag.Estados = new SelectList(_db.EstadoSolicituds.OrderBy(e => e.Orden), "EstadoId", "Nombre", estadoId);
            ViewBag.Departamentos = new SelectList(_db.Departamentos.Where(d => d.Activo).OrderBy(d => d.Nombre),
                "DepartamentoId", "Nombre", departamentoId);
            ViewBag.Q = q;

            return View(await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync());
        }

        // ---------- Detalle ----------
        public async Task<IActionResult> Detalle(int id)
        {
            var s = await _db.SolicitudCiudadanas
                .Include(x => x.Ciudadano).ThenInclude(c => c.Sector)
                .Include(x => x.Servicio).ThenInclude(v => v.Departamento)
                .Include(x => x.Estado)
                .FirstOrDefaultAsync(x => x.SolicitudId == id);
            if (s == null) return NotFound();

            var historial = await _db.AsignacionSolicituds
                .Include(a => a.Departamento).Include(a => a.Empleado).Include(a => a.AsignadoPorUsuario)
                .Where(a => a.SolicitudId == id)
                .OrderByDescending(a => a.FechaAsignacion).ToListAsync();

            var rol = await RolActual();
            return View(new SolicitudDetalleViewModel
            {
                Solicitud = s,
                Historial = historial,
                Actual = historial.FirstOrDefault(a => a.EsActual),
                PuedeAsignar = rol?.PuedeAsignar ?? false,
                PuedeAprobar = rol?.PuedeAprobar ?? false
            });
        }

        // ---------- Crear (solicitudes presenciales o telefónicas) ----------
        public IActionResult Crear()
        {
            CargarListas();
            return PartialView("_Form", new SolicitudCiudadana { Prioridad = 2, Canal = "Presencial" });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(SolicitudCiudadana m)
        {
            LimpiarNavegacion<SolicitudCiudadana>();
            ModelState.Remove("NumeroSolicitud");
            ModelState.Remove("EstadoId");

            var servicio = await _db.ServicioMunicipals.FindAsync(m.ServicioId);
            if (servicio == null) ModelState.AddModelError("ServicioId", "Seleccione un servicio");
            if (m.CiudadanoId == 0) ModelState.AddModelError("CiudadanoId", "Seleccione un ciudadano");

            if (!ModelState.IsValid) { CargarListas(m.CiudadanoId, m.ServicioId); return PartialView("_Form", m); }

            var siguiente = (await _db.SolicitudCiudadanas.MaxAsync(s => (int?)s.SolicitudId) ?? 0) + 1;
            m.NumeroSolicitud = $"SOL-{DateTime.Now.Year}-{siguiente:D6}";
            m.EstadoId = (await _db.EstadoSolicituds.FirstAsync(e => e.EsInicial)).EstadoId;
            m.FechaSolicitud = DateTime.Now;
            m.FechaLimite = DateTime.Now.AddDays(servicio.TiempoEstimadoDias);

            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // ---------- Editar datos generales ----------
        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.SolicitudCiudadanas.FindAsync(id);
            if (m == null) return NotFound();
            CargarListas(m.CiudadanoId, m.ServicioId);
            return PartialView("_Form", m);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(SolicitudCiudadana m)
        {
            LimpiarNavegacion<SolicitudCiudadana>();
            if (!ModelState.IsValid) { CargarListas(m.CiudadanoId, m.ServicioId); return PartialView("_Form", m); }

            var s = await _db.SolicitudCiudadanas.FindAsync(m.SolicitudId);
            if (s == null) return NotFound();

            s.Asunto = m.Asunto;
            s.Descripcion = m.Descripcion;
            s.Prioridad = m.Prioridad;
            s.Canal = m.Canal;
            s.DireccionIncidente = m.DireccionIncidente;
            s.Observaciones = m.Observaciones;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // ---------- Eliminar (solo recibidas y sin asignaciones) ----------
        [HttpPost]
        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var s = await _db.SolicitudCiudadanas.Include(x => x.Estado)
                .FirstOrDefaultAsync(x => x.SolicitudId == id);
            if (s == null) return NotFound();
            if (!s.Estado.EsInicial || await _db.AsignacionSolicituds.AnyAsync(a => a.SolicitudId == id))
                return Json(new { ok = false, mensaje = "Solo se pueden eliminar solicitudes recibidas y sin asignaciones." });

            _db.Notificacions.RemoveRange(_db.Notificacions.Where(n => n.SolicitudId == id));
            _db.SolicitudCiudadanas.Remove(s);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // ---------- Derivar ----------
        [HttpGet]
        public async Task<IActionResult> Derivar(int id)
        {
            ViewBag.SolicitudId = id;
            ViewBag.Departamentos = new SelectList(_db.Departamentos.Where(d => d.Activo).OrderBy(d => d.Nombre),
                "DepartamentoId", "Nombre");
            ViewBag.Empleados = await _db.Empleados.Where(e => e.Activo)
                .Select(e => new SelectListItem
                {
                    Value = e.EmpleadoId.ToString(),
                    Text = e.Nombres + " " + e.Apellidos,
                    Group = new SelectListGroup { Name = e.DepartamentoId.ToString() }
                }).ToListAsync();
            return PartialView("_Derivar");
        }

        [HttpPost]
        public async Task<IActionResult> Derivar(int solicitudId, int departamentoId, int? empleadoId, string comentario)
        {
            var rol = await RolActual();
            if (rol == null || !rol.PuedeAsignar)
                return Json(new { ok = false, mensaje = "Su rol no puede derivar solicitudes." });

            var s = await _db.SolicitudCiudadanas.Include(x => x.Estado).FirstOrDefaultAsync(x => x.SolicitudId == solicitudId);
            if (s == null) return NotFound();
            if (s.Estado.EsFinal)
                return Json(new { ok = false, mensaje = "La solicitud ya está cerrada." });

            var depto = await _db.Departamentos.FindAsync(departamentoId);
            if (depto == null || !depto.Activo)
                return Json(new { ok = false, mensaje = "Departamento no válido." });

            if (empleadoId != null &&
                !await _db.Empleados.AnyAsync(e => e.EmpleadoId == empleadoId && e.DepartamentoId == departamentoId && e.Activo))
                return Json(new { ok = false, mensaje = "El empleado no pertenece a ese departamento." });

            using var tx = await _db.Database.BeginTransactionAsync();

            var actuales = await _db.AsignacionSolicituds
                .Where(a => a.SolicitudId == solicitudId && a.EsActual).ToListAsync();
            foreach (var a in actuales) a.EsActual = false;
            await _db.SaveChangesAsync();   // libera el índice único

            _db.AsignacionSolicituds.Add(new AsignacionSolicitud
            {
                SolicitudId = solicitudId,
                DepartamentoId = departamentoId,
                EmpleadoId = empleadoId,
                AsignadoPorUsuarioId = UsuarioActualId,
                FechaAsignacion = DateTime.Now,
                FechaLimite = s.FechaLimite,
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim(),
                EsActual = true
            });
            s.EstadoId = (await _db.EstadoSolicituds.FirstAsync(e => e.Nombre == (empleadoId == null ? "Derivada" : "En proceso"))).EstadoId;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Notificar al jefe del departamento y al empleado asignado
            var destinatarios = new List<int>();
            if (depto.JefeEmpleadoId != null) destinatarios.Add(depto.JefeEmpleadoId.Value);
            if (empleadoId != null) destinatarios.Add(empleadoId.Value);
            await NotificarEmpleados(destinatarios, s, "Solicitud asignada",
                $"La solicitud {s.NumeroSolicitud} fue asignada a {depto.Nombre}.", "Alerta");

            return Json(new { ok = true });
        }

        // ---------- Cambiar estado ----------
        [HttpGet]
        public async Task<IActionResult> Estado(int id)
        {
            var s = await _db.SolicitudCiudadanas.FindAsync(id);
            if (s == null) return NotFound();
            ViewBag.SolicitudId = id;
            ViewBag.Estados = new SelectList(
                _db.EstadoSolicituds.Where(e => e.Activo && !e.EsInicial).OrderBy(e => e.Orden),
                "Nombre", "Nombre");
            return PartialView("_Estado");
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string estado, string observaciones)
        {
            var s = await _db.SolicitudCiudadanas.Include(x => x.Estado).FirstOrDefaultAsync(x => x.SolicitudId == id);
            var nuevo = await _db.EstadoSolicituds.FirstOrDefaultAsync(e => e.Nombre == estado && e.Activo);
            if (s == null || nuevo == null) return NotFound();

            if (s.Estado.EsFinal)
                return Json(new { ok = false, mensaje = "La solicitud ya está cerrada." });

            var rol = await RolActual();
            if (nuevo.EsFinal && (rol == null || !rol.PuedeAprobar))
                return Json(new { ok = false, mensaje = "Su rol no puede resolver o rechazar solicitudes." });

            if (nuevo.EsFinal && string.IsNullOrWhiteSpace(observaciones))
                return Json(new { ok = false, mensaje = "Escriba una observación para cerrar la solicitud." });

            s.EstadoId = nuevo.EstadoId;
            if (!string.IsNullOrWhiteSpace(observaciones)) s.Observaciones = observaciones.Trim();

            if (nuevo.EsFinal)
            {
                s.FechaResolucion = DateTime.Now;
                var actual = await _db.AsignacionSolicituds.FirstOrDefaultAsync(a => a.SolicitudId == id && a.EsActual);
                if (actual != null) actual.FechaCompletado = DateTime.Now;
            }
            await _db.SaveChangesAsync();

            // Avisar al jefe y empleado de la asignación vigente
            var vigente = await _db.AsignacionSolicituds.Include(a => a.Departamento)
                .FirstOrDefaultAsync(a => a.SolicitudId == id && a.EsActual);
            var dest = new List<int>();
            if (vigente?.EmpleadoId != null) dest.Add(vigente.EmpleadoId.Value);
            if (vigente?.Departamento?.JefeEmpleadoId != null) dest.Add(vigente.Departamento.JefeEmpleadoId.Value);
            await NotificarEmpleados(dest, s, $"Solicitud {nuevo.Nombre.ToLower()}",
                $"La solicitud {s.NumeroSolicitud} cambió a: {nuevo.Nombre}.",
                nuevo.EsFinal ? "Resolucion" : "Informativa");

            return Json(new { ok = true });
        }

        // ---------- Auxiliares ----------
        private async Task<Rol> RolActual()
        {
            var u = await _db.Usuarios.Include(x => x.Rol).FirstOrDefaultAsync(x => x.UsuarioId == UsuarioActualId);
            return u?.Rol;
        }

        // Crea notificaciones internas para las cuentas de los empleados indicados
        private async Task NotificarEmpleados(IEnumerable<int> empleadoIds, SolicitudCiudadana s,
            string titulo, string mensaje, string tipo)
        {
            var ids = empleadoIds.Distinct().ToList();
            var cuentas = await _db.Usuarios
                .Where(u => u.EmpleadoId != null && ids.Contains(u.EmpleadoId.Value) && u.Activo)
                .Select(u => u.UsuarioId).ToListAsync();

            foreach (var uid in cuentas)
                _db.Notificacions.Add(new Notificacion
                {
                    UsuarioId = uid,
                    SolicitudId = s.SolicitudId,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = tipo,
                    Canal = "Sistema",
                    Leida = false,
                    FechaEnvio = DateTime.Now
                });
            if (cuentas.Count > 0) await _db.SaveChangesAsync();
        }

        private void CargarListas(int? ciudadano = null, int? servicio = null)
        {
            ViewBag.Ciudadanos = new SelectList(
                _db.Ciudadanos.Where(c => c.Activo || c.CiudadanoId == ciudadano)
                    .Select(c => new { c.CiudadanoId, Nombre = c.Nombres + " " + c.Apellidos + " (" + c.Cedula + ")" }),
                "CiudadanoId", "Nombre", ciudadano);
            ViewBag.Servicios = new SelectList(
                _db.ServicioMunicipals.Where(s => s.Activo || s.ServicioId == servicio).OrderBy(s => s.Nombre),
                "ServicioId", "Nombre", servicio);
            ViewBag.Prioridades = new SelectList(new[]
            {
                new { Id = 1, Nombre = "Alta" }, new { Id = 2, Nombre = "Media" }, new { Id = 3, Nombre = "Baja" }
            }, "Id", "Nombre");
            ViewBag.Canales = new SelectList(new[] { "Web", "Presencial", "Telefono", "App", "Correo" });
        }
    }
}