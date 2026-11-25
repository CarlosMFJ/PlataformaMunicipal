using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize]
    public class CiudadanosController : AppControllerBase
    {
        public CiudadanosController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.Ciudadanos.Include(c => c.Sector)
                .OrderBy(c => c.Apellidos).ToListAsync());

        public IActionResult Crear()
        {
            CargarListas();
            return PartialView("_Form", new Ciudadano { Activo = true });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Ciudadano m)
        {
            LimpiarNavegacion<Ciudadano>();
            Normalizar(m);
            await Validar(m);
            if (!ModelState.IsValid) { CargarListas(m.SectorId); return PartialView("_Form", m); }

            await CompletarMunicipio(m);
            m.FechaRegistro = DateTime.Now;
            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.Ciudadanos.FindAsync(id);
            if (m == null) return NotFound();
            CargarListas(m.SectorId);
            return PartialView("_Form", m);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Ciudadano m)
        {
            LimpiarNavegacion<Ciudadano>();
            Normalizar(m);
            await Validar(m);
            if (!ModelState.IsValid) { CargarListas(m.SectorId); return PartialView("_Form", m); }

            await CompletarMunicipio(m);
            _db.Update(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var c = await _db.Ciudadanos.FindAsync(id);
            if (c == null) return NotFound();
            c.Activo = false;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // La BD guarda cédula y teléfono sin guiones
        private static void Normalizar(Ciudadano m)
        {
            m.Cedula = (m.Cedula ?? "").Replace("-", "").Trim();
            if (!string.IsNullOrWhiteSpace(m.Telefono))
                m.Telefono = m.Telefono.Replace("-", "").Trim();
        }

        private async Task Validar(Ciudadano m)
        {
            if (m.Cedula.Length != 11 || !m.Cedula.All(char.IsDigit))
                ModelState.AddModelError("Cedula", "La cédula debe tener exactamente 11 dígitos");
            else if (await _db.Ciudadanos.AnyAsync(c => c.Cedula == m.Cedula && c.CiudadanoId != m.CiudadanoId))
                ModelState.AddModelError("Cedula", "Ya existe un ciudadano con esa cédula");

            if (!string.IsNullOrWhiteSpace(m.Telefono) && (m.Telefono.Length != 10 || !m.Telefono.All(char.IsDigit)))
                ModelState.AddModelError("Telefono", "El teléfono debe tener 10 dígitos");

            if (m.Sexo != null && m.Sexo != "M" && m.Sexo != "F")
                ModelState.AddModelError("Sexo", "Seleccione M o F");

            if (m.FechaNacimiento != null && m.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
                ModelState.AddModelError("FechaNacimiento", "La fecha no puede ser futura");

            if (m.SectorId != null && !await _db.Sectors.AnyAsync(s => s.SectorId == m.SectorId && s.Activo))
                ModelState.AddModelError("SectorId", "Seleccione un sector válido");
        }

        // El municipio se toma del sector elegido
        private async Task CompletarMunicipio(Ciudadano m)
        {
            if (m.SectorId != null)
                m.Municipio = (await _db.Sectors.FindAsync(m.SectorId))?.Municipio;
        }

        private void CargarListas(int? sector = null)
        {
            ViewBag.Sectores = new SelectList(
                _db.Sectors.Where(s => s.Activo || s.SectorId == sector).OrderBy(s => s.Nombre),
                "SectorId", "Nombre", sector);
            ViewBag.Sexos = new SelectList(new[]
            {
                new { Id = "M", Nombre = "Masculino" }, new { Id = "F", Nombre = "Femenino" }
            }, "Id", "Nombre");
        }
    }
}