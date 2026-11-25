using System.ComponentModel.DataAnnotations;

namespace PlataformaMunicipal.ViewModels
{
    public class UsuarioFormViewModel
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Seleccione un rol")]
        [Display(Name = "Rol")]
        public int RolId { get; set; }

        [Display(Name = "Empleado")]
        public int? EmpleadoId { get; set; }

        [Display(Name = "Ciudadano")]
        public int? CiudadanoId { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre de usuario")]
        [StringLength(50)]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "Ingrese el correo")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(120)]
        public string Correo { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        public bool Activo { get; set; } = true;
        public bool Bloqueado { get; set; }
    }
}