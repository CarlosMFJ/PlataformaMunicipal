using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize(Roles = "Administrador,Director")]
    public class DepartamentosController : Controller
    {
        private readonly AppDbContext _db;
        public DepartamentosController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var lista = await _db.Departamentos
                .Include(d => d.JefeEmpleado)
                .OrderBy(d => d.Nombre).ToListAsync();
            return View(lista);
        }

        public IActionResult Crear()
        {
            CargarJefes();
            return PartialView("_Form", new Departamento { Activo = true });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Departamento m)
        {
            LimpiarNavegacion();
            if (await _db.Departamentos.AnyAsync(d => d.Codigo == m.Codigo))
                ModelState.AddModelError("Codigo", "Ya existe un departamento con ese código");
            if (await _db.Departamentos.AnyAsync(d => d.Nombre == m.Nombre))
                ModelState.AddModelError("Nombre", "Ya existe un departamento con ese nombre");

            if (!ModelState.IsValid) { CargarJefes(m.JefeEmpleadoId); return PartialView("_Form", m); }

            m.FechaCreacion = DateTime.Now;
            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.Departamentos.FindAsync(id);
            if (m == null) return NotFound();
            CargarJefes(m.JefeEmpleadoId);
            return PartialView("_Form", m);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Departamento m)
        {
            LimpiarNavegacion();
            if (await _db.Departamentos.AnyAsync(d => d.Codigo == m.Codigo && d.DepartamentoId != m.DepartamentoId))
                ModelState.AddModelError("Codigo", "Ya existe un departamento con ese código");
            if (await _db.Departamentos.AnyAsync(d => d.Nombre == m.Nombre && d.DepartamentoId != m.DepartamentoId))
                ModelState.AddModelError("Nombre", "Ya existe un departamento con ese nombre");

            if (!ModelState.IsValid) { CargarJefes(m.JefeEmpleadoId); return PartialView("_Form", m); }

            _db.Update(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var d = await _db.Departamentos.FindAsync(id);
            if (d == null) return NotFound();
            d.Activo = false;                    // eliminación lógica
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private void CargarJefes(int? seleccionado = null)
        {
            ViewBag.Jefes = new SelectList(
                _db.Empleados.Where(e => e.Activo)
                    .Select(e => new { e.EmpleadoId, Nombre = e.Nombres + " " + e.Apellidos }),
                "EmpleadoId", "Nombre", seleccionado);
        }

        // Evita que las propiedades de navegación (listas/objetos) invaliden el ModelState
        private void LimpiarNavegacion()
        {
            ModelState.Remove("JefeEmpleado");
            ModelState.Remove("Empleados");
            ModelState.Remove("ServicioMunicipals");
            ModelState.Remove("AsignacionSolicituds");
        }
    }
}
