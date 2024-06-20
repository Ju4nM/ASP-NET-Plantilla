using System.ComponentModel.DataAnnotations;

namespace sigecendis_api.DTOs.Role {
    public class ToggleRoleDto {
        [Required(ErrorMessage = "No se ha proporcionado el identificador del rol.")]
        [Range(0, int.MaxValue, ErrorMessage = "El identificador no tiene el formato adecuado.")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "No se ha proporcionado el identificador del usuario.")]
        [Range(0, int.MaxValue, ErrorMessage = "El identificador no tiene el formato adecuado.")]
        public int IdUsuario { get; set; }
    }
}
