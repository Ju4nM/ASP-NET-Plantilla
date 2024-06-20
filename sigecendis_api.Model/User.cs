using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sigecendis_api.Model {
  public class User {
    [JsonPropertyName("id")]
    public int IdUsuario { get; set; }

    [JsonPropertyName("name")]
    public string Nombres { get; set; }

    [JsonPropertyName("firstName")]
    public string PrimerApellido { get; set; }

    [JsonPropertyName("secondName")]
    public string SegundoApellido { get; set; }

    [JsonPropertyName("email")]
    public string EmailUsuario { get; set; }

    [JsonIgnore]
    public string Contrasena { get; set; }

    [JsonIgnore]
    public string RefreshToken { get; set; }

    [JsonIgnore]
    public bool UsuarioActivo { get; set; }

    [JsonPropertyName("role")]
    public Role Rol { get; set; }
  }
}
