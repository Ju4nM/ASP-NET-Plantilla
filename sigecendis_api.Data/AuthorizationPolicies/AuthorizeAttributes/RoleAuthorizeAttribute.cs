using Microsoft.AspNetCore.Authorization;

namespace sigecendis_api.Data.AuthorizationPolicies.AuthorizeAttributes {

  // Llaves de roles registrados en la base de datos
  public static class ROLES {
    public const string COORDINATION = "coordination";
    public const string CENDI_MANAGEMENT = "cendiManagement";
    public const string GENERAL = "general";
  }

  public class RoleAuthorizeAttribute: AuthorizeAttribute {
    public static string rolePrefix = "ROLE%";

    // Los nombres de las politicas las agrega con un prefijo para distinguirlas en el provider
    // ejem => ROLE%admin%general%...
    public RoleAuthorizeAttribute(params string[] roleKeys) => Policy = $"{ rolePrefix }{ String.Join('%', roleKeys) }";

    public static string[] GetRoleKeysFromPolicy(string policyName) => policyName.Replace(rolePrefix, "").Split('%');
  }
}
