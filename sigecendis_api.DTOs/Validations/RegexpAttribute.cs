using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace sigecendis_api.DTOs.Validations {

  // Copia del ya existente RegularExpressionAttribute
  // El ya existente no permite llamarlo mas de una vez, este si (AlloMultiple = true)
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
  public class RegexpAttribute : ValidationAttribute {

    private Regex? Regex { get; set; }
    private string Pattern { get; set; }

    public RegexpAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string pattern) : base() {
      Pattern = pattern;
    }

    public override bool IsValid(object? value) {

      SetupRegex();

      // Convert the value to a string
      string? stringValue = Convert.ToString(value);

      // Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
      if (string.IsNullOrEmpty(stringValue)) {
        return true;
      }

      foreach (ValueMatch m in Regex!.EnumerateMatches(stringValue)) {
        // We are looking for an exact match, not just a search hit. This matches what
        // the RegularExpressionValidator control does
        return m.Index == 0 && m.Length == stringValue.Length;
      }

      return false;
    }

    private void SetupRegex() {
      if (Regex != null) return;
      if (string.IsNullOrEmpty(Pattern)) throw new InvalidOperationException("El patron es invalido.");
      Regex = new Regex(Pattern, RegexOptions.IgnoreCase);
    }
  }
}
