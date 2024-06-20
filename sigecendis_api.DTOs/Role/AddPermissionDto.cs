
using System.ComponentModel.DataAnnotations;

namespace sigecendis_api.DTOs.Role {
    public class AddPermissionDto {

        [Required(ErrorMessage = "El identificador del rol no fue proporcionado.")]
        [Range(0, int.MaxValue, ErrorMessage = "El identificador no tiene el formato adecuado.")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "El identificador del permiso no fue proporcionado")]
        [Range(0, int.MaxValue, ErrorMessage = "El identificador no tiene el formato adecuado.")]
        public int PermissionId { get; set; }
    }
}
