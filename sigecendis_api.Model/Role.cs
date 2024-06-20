using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sigecendis_api.Model {
  public class Role {
    [JsonPropertyName("id")]
    public int IdRol { get; set; }

    [JsonPropertyName("name")]
    public string NombreRol { get; set; }

    [JsonPropertyName("description")]
    public string Descripcion { get; set; }

    [JsonPropertyName("key")]
    public string Llave { get; set; }

    [JsonIgnore]
    public int Nivel { get; set; }

    //[JsonIgnore]
    public bool RolActivo { get; set; }
  }
}
