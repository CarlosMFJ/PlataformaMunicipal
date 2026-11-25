using System.ComponentModel.DataAnnotations;

namespace PlataformaMunicipal.ViewModels
{
    public class SolicitudViewModel : CiudadanoFormBase
    {
        [Required(ErrorMessage = "Detalle lo que necesita")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Debe tener entre 10 y 2000 caracteres")]
        [Display(Name = "Detalle de su solicitud")]
        public string Descripcion { get; set; }

        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        [Display(Name = "Observaciones (opcional)")]
        public string Observaciones { get; set; }
    }
}