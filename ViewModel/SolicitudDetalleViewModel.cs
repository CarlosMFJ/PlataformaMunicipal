using PlataformaMunicipal.Models;

namespace PlataformaMunicipal.ViewModels
{
    public class SolicitudDetalleViewModel
    {
        public SolicitudCiudadana Solicitud { get; set; }
        public List<AsignacionSolicitud> Historial { get; set; } = new();
        public AsignacionSolicitud Actual { get; set; }
        public bool PuedeAsignar { get; set; }
        public bool PuedeAprobar { get; set; }
    }
}