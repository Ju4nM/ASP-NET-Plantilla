using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.DTOs.Auth {
  public class RefreshTokenDto {

    [Required(ErrorMessage = "El refreshToken no ha sido proporcionado.")]
    public string RefreshToken { get; set; }
  }
}
