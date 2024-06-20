using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.Data.AuthorizationPolicies.AuthorizationRequirements {
  public class RoleRequirement : IAuthorizationRequirement {
    public string[] Roles { get; set; }

    public RoleRequirement(string[] roles) => Roles = roles;
  }
}
