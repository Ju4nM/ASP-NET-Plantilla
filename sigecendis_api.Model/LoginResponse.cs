using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.Model {
  public class LoginResponse {

    public User User { get; set; }
    public Role Role { get; set; }
    public string accessToken { get; set; }
    public string refreshToken { get; set; }
  }
}
