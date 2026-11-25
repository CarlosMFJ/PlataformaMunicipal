using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize]
    public class ServiciosController : AppControllerBase
    {
        public ServiciosController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.ServicioMunicipals.Include(s => s.Departamento)
                .OrderBy(s => s.Codigo).ToListAsync());

        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public IActionResult Crear()
        {
            CargarListas();
            return PartialView("_Form", new ServicioMunicipal { Activo = true, TiempoEstimadoDias = 5 });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Crear(ServicioMunicipal m)
        {
            LimpiarNavegacion<ServicioMunicipal>();
            await Validar(m);
            if (!ModelState.IsValid) { CargarListas(m.DepartamentoId); return PartialView("_Form", m); }

            m.FechaCreacion = DateTime.Now;
            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.ServicioMunicipals.FindAsync(id);
            if (m == null) return NotFound();
            CargarListas(m.DepartamentoId);
            return PartialView("_Form", m);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Editar(ServicioMunicipal m)
        {
            LimpiarNavegacion<ServicioMunicipal>();
            await Validar(m);
            if (!ModelState.IsValid) { CargarListas(m.DepartamentoId); return PartialView("_Form", m); }

            _db.Update(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Director,Jefe de Departamento")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var s = await _db.ServicioMunicipals.FindAsync(id);
            if (s == null) return NotFound();
            s.Activo = false;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private async Task Validar(ServicioMunicipal m)
        {
            if (await _db.ServicioMunicipals.AnyAsync(s => s.Codigo == m.Codigo && s.ServicioId != m.ServicioId))
                ModelState.AddModelError("Codigo", "Ya existe un servicio con ese código");
            if (m.Costo < 0) ModelState.AddModelError("Costo", "El costo no puede ser negativo");
            if (m.TiempoEstimadoDias <= 0) ModelState.AddModelError("TiempoEstimadoDias", "Debe ser mayor que 0");
        }

        private void CargarListas(int? depto = null) =>
            ViewBag.Departamentos = new SelectList(
                _db.Departamentos.Where(d => d.Activo).OrderBy(d => d.Nombre),
                "DepartamentoId", "Nombre", depto);
    }
}
