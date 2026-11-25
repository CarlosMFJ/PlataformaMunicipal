using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.Controllers
{
    public abstract class AppControllerBase : Controller
    {
        protected readonly AppDbContext _db;
        protected AppControllerBase(AppDbContext db) => _db = db;

        protected int UsuarioActualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Quita del ModelState las propiedades de navegación (objetos y listas)
        protected void LimpiarNavegacion<T>()
        {
            var et = _db.Model.FindEntityType(typeof(T));
            if (et == null) return;
            foreach (var n in et.GetNavigations())
                ModelState.Remove(n.Name);
        }

        
    }
}