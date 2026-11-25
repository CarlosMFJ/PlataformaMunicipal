using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    [Authorize(Roles = "Administrador,Director")]
    public class EmpleadosController : AppControllerBase
    {
        public EmpleadosController(AppDbContext db) : base(db) { }

        public async Task<IActionResult> Index() =>
            View(await _db.Empleados.Include(e => e.Departamento)
                .OrderBy(e => e.Apellidos).ToListAsync());

        public IActionResult Crear()
        {
            CargarListas();
            return PartialView("_Form", new Empleado
            {
                Activo = true,
                FechaIngreso = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Empleado m)
        {
            LimpiarNavegacion<Empleado>();
            await Validar(m);
            if (!ModelState.IsValid) { CargarListas(m.DepartamentoId); return PartialView("_Form", m); }

            _db.Add(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var m = await _db.Empleados.FindAsync(id);
            if (m == null) return NotFound();
            CargarListas(m.DepartamentoId);
            return PartialView("_Form", m);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Empleado m)
        {
            LimpiarNavegacion<Empleado>();
            await Validar(m);
            if (!ModelState.IsValid) { CargarListas(m.DepartamentoId); return PartialView("_Form", m); }

            _db.Update(m);
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var e = await _db.Empleados.FindAsync(id);
            if (e == null) return NotFound();
            e.Activo = false;
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private async Task Validar(Empleado m)
        {
            if (await _db.Empleados.AnyAsync(e => e.Cedula == m.Cedula && e.EmpleadoId != m.EmpleadoId))
                ModelState.AddModelError("Cedula", "Ya existe un empleado con esa cédula");
            if (await _db.Empleados.AnyAsync(e => e.Correo == m.Correo && e.EmpleadoId != m.EmpleadoId))
                ModelState.AddModelError("Correo", "Ya existe un empleado con ese correo");
            if (m.Salario < 0)
                ModelState.AddModelError("Salario", "El salario no puede ser negativo");
        }

        private void CargarListas(int? depto = null) =>
            ViewBag.Departamentos = new SelectList(
                _db.Departamentos.Where(d => d.Activo).OrderBy(d => d.Nombre),
                "DepartamentoId", "Nombre", depto);
    }
}