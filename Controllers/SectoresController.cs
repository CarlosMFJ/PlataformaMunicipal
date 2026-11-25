using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
    public class SectoresController : AppControllerBase
    {
        public SectoresController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.Sectors.OrderBy(s => s.Nombre).ToListAsync());

        [Authorize(Roles = "Administrador,Director")]
        public async Task<IActionResult> Crear()
        {
            return PartialView("_Form", new Sector
            {
                Activo = true,
                Municipio = "Galvan",
                Codigo = await SiguienteCodigo()   // solo para mostrarlo
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director")]
        public async Task<IActionResult> Crear(Sector m)
        {
            LimpiarNavegacion<Sector>();
            ModelState.Remove("Codigo");   // el código lo asigna el sistema

            if (await _db.Sectors.AnyAsync(s => s.Nombre == m.Nombre))
                ModelState.AddModelError("Nombre", "Ya existe un sector con ese nombre");

            if (!ModelState.IsValid)
            {
                m.Codigo = await SiguienteCodigo();
                return PartialView("_Form", m);
            }

            m.Codigo = await SiguienteCodigo();   // se calcula de nuevo al guardar
            m.FechaCreacion = DateTime.Now;
            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [Authorize(Roles = "Administrador,Director")]
        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.Sectors.FindAsync(id);
            return m == null ? NotFound() : PartialView("_Form", m);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director")]
        public async Task<IActionResult> Editar(Sector m)
        {
            LimpiarNavegacion<Sector>();

            if (await _db.Sectors.AnyAsync(s => s.Nombre == m.Nombre && s.SectorId != m.SectorId))
                ModelState.AddModelError("Nombre", "Ya existe un sector con ese nombre");
            if (!ModelState.IsValid) return PartialView("_Form", m);

            var s = await _db.Sectors.FindAsync(m.SectorId);
            if (s == null) return NotFound();

            // El código no se modifica
            s.Nombre = m.Nombre;
            s.Municipio = m.Municipio;
            s.Descripcion = m.Descripcion;
            s.Activo = m.Activo;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var s = await _db.Sectors.FindAsync(id);
            if (s == null) return NotFound();
            s.Activo = false;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // SEC-001, SEC-002... toma el mayor número existente y suma 1
        private async Task<string> SiguienteCodigo()
        {
            var codigos = await _db.Sectors
                .Where(s => s.Codigo.StartsWith("SEC-"))
                .Select(s => s.Codigo).ToListAsync();

            int max = 0;
            foreach (var c in codigos)
                if (int.TryParse(c.Substring(4), out var n) && n > max) max = n;

            return $"SEC-{max + 1:D3}";
        }
    }
}