using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.DTOs.Role {
    public class UpdateRoleDto {
        [Required(ErrorMessage = "El nombre no ha sido proporcionado.")]
        [MinLength(2, ErrorMessage = "El nombre al menos debe tener 2 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripcion no ha sido proporcionada.")]
        [MaxLength(250, ErrorMessage = "La descripcion puede tener maximo 250 caracteres.")]
        public string Descripcion { get; set; }

    }
}
