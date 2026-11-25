using System.ComponentModel.DataAnnotations;

namespace PlataformaMunicipal.ViewModels
{
    // Datos del ciudadano (comunes a queja y solicitud)
    public abstract class CiudadanoFormBase
    {
        [Required(ErrorMessage = "Ingrese su cédula")]
        [RegularExpression(@"^\d{3}-?\d{7}-?\d$", ErrorMessage = "La cédula debe tener exactamente 11 dígitos")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "Ingrese sus nombres")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Debe tener entre 2 y 80 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s'\-]+$", ErrorMessage = "Solo se permiten letras")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Ingrese sus apellidos")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Debe tener entre 2 y 80 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s'\-]+$", ErrorMessage = "Solo se permiten letras")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Ingrese un teléfono")]
        [RegularExpression(@"^\d{3}-?\d{3}-?\d{4}$", ErrorMessage = "El teléfono debe tener 10 dígitos")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(120, ErrorMessage = "Máximo 120 caracteres")]
        public string Correo { get; set; }

        [Display(Name = "Sector")]
        public int? SectorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción")]
        [Display(Name = "Servicio")]
        public int ServicioId { get; set; }

        [Required(ErrorMessage = "Ingrese el asunto")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Debe tener entre 5 y 150 caracteres")]
        public string Asunto { get; set; }

        // Campo trampa anti-spam
        public string Sitio { get; set; }
    }

    public class QuejaViewModel : CiudadanoFormBase
    {
        [Required(ErrorMessage = "Indique dónde ocurre el problema")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "Debe tener entre 5 y 250 caracteres")]
        [Display(Name = "Dirección donde ocurre el problema")]
        public string DireccionIncidente { get; set; }

        [Required(ErrorMessage = "Describa el problema")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "Debe tener entre 20 y 2000 caracteres")]
        [Display(Name = "¿Qué está ocurriendo?")]
        public string Descripcion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha en que ocurrió (opcional)")]
        public DateTime? FechaIncidente { get; set; }
    }

   
}