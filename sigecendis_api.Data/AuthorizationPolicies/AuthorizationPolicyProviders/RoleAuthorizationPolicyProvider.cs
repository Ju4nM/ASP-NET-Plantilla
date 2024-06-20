using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using sigecendis_api.Data.AuthorizationPolicies.AuthorizationRequirements;
using static sigecendis_api.Data.AuthorizationPolicies.AuthorizeAttributes.RoleAuthorizeAttribute;

namespace sigecendis_api.Data.AuthorizationPolicies.AuthorizationPolicyProviders {
  public class RoleAuthorizationPolicyProvider : IAuthorizationPolicyProvider {

    public DefaultAuthorizationPolicyProvider OriginalPolicyProvider { get; set; }
    public RoleAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) {
      OriginalPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }
    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) {
      // En caso de que no sea una politica de Rol deja que sea manejada por el defaultProvider
      if (!policyName.StartsWith(rolePrefix, StringComparison.OrdinalIgnoreCase)) return await OriginalPolicyProvider.GetPolicyAsync(policyName);

      string[] roles = GetRoleKeysFromPolicy(policyName);

      RoleRequirement requirement = new (roles);

      // Agrega el requerimiento en tiempo de ejecucion
      return new AuthorizationPolicyBuilder().AddRequirements(requirement).Build();
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() {
      return OriginalPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() {
      return OriginalPolicyProvider.GetFallbackPolicyAsync();
    }

  }
}
