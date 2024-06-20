using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.Model {
  public class RefreshTokenResponse {
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
  }
}
