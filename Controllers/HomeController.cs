using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaMunicipal.Models;
using PlataformaMunicipal.ViewModels;

namespace PlataformaMunicipal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.Now;
            var vm = new DashboardViewModel
            {
                Total = await _db.SolicitudCiudadanas.CountAsync(),
                Resueltas = await _db.SolicitudCiudadanas.CountAsync(s => s.Estado.Nombre == "Resuelta"),
                EnProceso = await _db.SolicitudCiudadanas.CountAsync(s => s.Estado.Nombre == "En proceso"),
                Vencidas = await _db.SolicitudCiudadanas.CountAsync(s =>
                    s.FechaLimite < ahora && s.FechaResolucion == null && !s.Estado.EsFinal),

                PorEstado = await _db.SolicitudCiudadanas
                    .GroupBy(s => new { s.Estado.Nombre, s.Estado.Color })
                    .Select(g => new ChartItem { Etiqueta = g.Key.Nombre, Color = g.Key.Color, Valor = g.Count() })
                    .ToListAsync(),

                PorDepartamento = await _db.AsignacionSolicituds
                    .Where(a => a.EsActual)
                    .GroupBy(a => a.Departamento.Nombre)
                    .Select(g => new ChartItem { Etiqueta = g.Key, Valor = g.Count() })
                    .ToListAsync(),

                PorSector = await _db.SolicitudCiudadanas
                    .GroupBy(s => s.Ciudadano.Sector != null ? s.Ciudadano.Sector.Nombre : "Sin sector")
                    .Select(g => new ChartItem { Etiqueta = g.Key, Valor = g.Count() })
                    .ToListAsync()
            };
            return View(vm);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}