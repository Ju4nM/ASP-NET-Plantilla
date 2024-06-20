using System.ComponentModel.DataAnnotations;

namespace sigecendis_api.DTOs.Validations {

  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
  public class IsOptionalAttribute: ValidationAttribute {
    public override bool IsValid(object? value) => true;
  }
}
