using System.ComponentModel.DataAnnotations;

namespace PlataformaMunicipal.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese su usuario")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Ingrese su contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }
    }
}