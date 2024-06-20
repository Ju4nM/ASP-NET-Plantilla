using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.DTOs.Role {
    public class CreateRoleDto {
        [Required(ErrorMessage = "El nombre no ha sido proporcionado.")]
        [MinLength(2, ErrorMessage = "El nombre al menos debe tener 2 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripcion no ha sido proporcionada.")]
        [MaxLength(250, ErrorMessage = "La descripcion puede tener maximo 250 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "No se ha proporcionado el identificador del usuario creador del rol.")]
        [Range(0, int.MaxValue, ErrorMessage = "El identificador del usuario creador del rol no tiene el formato adecuado.")]
        public int CreadoPor { get; set; }

    }
}
