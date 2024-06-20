using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.DTOs.Auth {
    public class LoginDto {

        [Required(ErrorMessage = "El correo no ha sido proporcionado.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña no ha sido proporcionada.")]
        public string Password { get; set; }
    }
}
