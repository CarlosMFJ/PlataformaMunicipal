using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;
using PlataformaMunicipal.ViewModels;

namespace PlataformaMunicipal.Controllers
{
    [AllowAnonymous]
    public class PublicoController : Controller
    {
        private readonly AppDbContext _db;
        public PublicoController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var servicios = await _db.ServicioMunicipals
                .Include(s => s.Departamento)
                .Where(s => s.Activo)
                .OrderBy(s => s.Nombre).ToListAsync();
            return View(servicios);
        }

        // ================= QUEJA =================
        [HttpGet]
        public IActionResult Queja(int? servicioId)
        {
            CargarListas(servicioId, null);
            return View(new QuejaViewModel { ServicioId = servicioId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Queja(QuejaViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.Sitio)) return RedirectToAction("Index");

            if (vm.FechaIncidente != null && vm.FechaIncidente > DateTime.Today)
                ModelState.AddModelError("FechaIncidente", "La fecha no puede ser futura");

            var servicio = await ValidarComunes(vm);
            if (!ModelState.IsValid)
            {
                CargarListas(vm.ServicioId, vm.SectorId);
                return View(vm);
            }

            var descripcion = vm.Descripcion.Trim();
            if (vm.FechaIncidente != null)
                descripcion += $"\n\nFecha del incidente: {vm.FechaIncidente:dd/MM/yyyy}";

            var numero = await Registrar(vm, servicio, "Queja", "QUE",
                descripcion, vm.DireccionIncidente.Trim(), null);
            return RedirectToAction("Confirmacion", new { numero, tipo = "Queja" });
        }

        // ================= SOLICITUD =================
        [HttpGet]
        public IActionResult Solicitud(int? servicioId)
        {
            CargarListas(servicioId, null);
            return View(new SolicitudViewModel { ServicioId = servicioId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Solicitud(SolicitudViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.Sitio)) return RedirectToAction("Index");

            var servicio = await ValidarComunes(vm);
            if (!ModelState.IsValid)
            {
                CargarListas(vm.ServicioId, vm.SectorId);
                return View(vm);
            }

            var numero = await Registrar(vm, servicio, "Solicitud", "SOL",
                vm.Descripcion.Trim(), null,
                string.IsNullOrWhiteSpace(vm.Observaciones) ? null : vm.Observaciones.Trim());
            return RedirectToAction("Confirmacion", new { numero, tipo = "Solicitud" });
        }

        public IActionResult Confirmacion(string numero, string tipo)
        {
            ViewBag.Numero = numero;
            ViewBag.Tipo = tipo;
            return View();
        }

        // ================= SEGUIMIENTO =================
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Consultar(string numero, string cedula)
        {
            numero = (numero ?? "").Trim().ToUpper();
            cedula = (cedula ?? "").Replace("-", "").Trim();

            var s = await _db.SolicitudCiudadanas
                .Include(x => x.Estado).Include(x => x.Servicio)
                .FirstOrDefaultAsync(x => x.NumeroSolicitud == numero && x.Ciudadano.Cedula == cedula);

            return View("Resultado", s);
        }

        // ================= AUXILIARES =================
        private async Task<ServicioMunicipal> ValidarComunes(CiudadanoFormBase vm)
        {
            var servicio = await _db.ServicioMunicipals
                .FirstOrDefaultAsync(s => s.ServicioId == vm.ServicioId && s.Activo);
            if (servicio == null)
                ModelState.AddModelError("ServicioId", "Seleccione una opción válida");

            if (vm.SectorId != null &&
                !await _db.Sectors.AnyAsync(s => s.SectorId == vm.SectorId && s.Activo))
                ModelState.AddModelError("SectorId", "Seleccione un sector válido");

            return servicio;
        }

        private async Task<string> Registrar(CiudadanoFormBase vm, ServicioMunicipal servicio,
            string tipo, string prefijo, string descripcion, string direccion, string observaciones)
        {
            var cedula = vm.Cedula.Replace("-", "").Trim();
            var telefono = vm.Telefono.Replace("-", "").Trim();

            var ciudadano = await _db.Ciudadanos.FirstOrDefaultAsync(c => c.Cedula == cedula);
            if (ciudadano == null)
            {
                ciudadano = new Ciudadano
                {
                    Cedula = cedula,
                    Nombres = vm.Nombres.Trim(),
                    Apellidos = vm.Apellidos.Trim(),
                    Telefono = telefono,
                    Correo = string.IsNullOrWhiteSpace(vm.Correo) ? null : vm.Correo.Trim(),
                    SectorId = vm.SectorId,
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };
                _db.Ciudadanos.Add(ciudadano);
                await _db.SaveChangesAsync();
            }
            else if (vm.SectorId != null)
            {
                ciudadano.SectorId = vm.SectorId;
                await _db.SaveChangesAsync();
            }

            var estadoInicial = await _db.EstadoSolicituds.FirstAsync(e => e.EsInicial);
            var siguiente = (await _db.SolicitudCiudadanas.MaxAsync(s => (int?)s.SolicitudId) ?? 0) + 1;

            var solicitud = new SolicitudCiudadana
            {
                NumeroSolicitud = $"{prefijo}-{DateTime.Now.Year}-{siguiente:D6}",
                Tipo = tipo,
                CiudadanoId = ciudadano.CiudadanoId,
                ServicioId = servicio.ServicioId,
                EstadoId = estadoInicial.EstadoId,
                Asunto = vm.Asunto.Trim(),
                Descripcion = descripcion,
                Prioridad = tipo == "Queja" ? (byte)2 : (byte)3,
                Canal = "Web",
                DireccionIncidente = direccion,
                Observaciones = observaciones,
                FechaSolicitud = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(servicio.TiempoEstimadoDias)
            };
            _db.SolicitudCiudadanas.Add(solicitud);
            await _db.SaveChangesAsync();
            return solicitud.NumeroSolicitud;
        }

        private void CargarListas(int? servicio, int? sector)
        {
            ViewBag.Servicios = new SelectList(
                _db.ServicioMunicipals.Where(s => s.Activo).OrderBy(s => s.Nombre),
                "ServicioId", "Nombre", servicio);
            ViewBag.Sectores = new SelectList(
                _db.Sectors.Where(s => s.Activo).OrderBy(s => s.Nombre),
                "SectorId", "Nombre", sector);
        }
    }
}