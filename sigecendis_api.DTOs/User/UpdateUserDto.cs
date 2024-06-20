using sigecendis_api.DTOs.Validations;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace sigecendis_api.DTOs.User {
  public class UpdateUserDto {

    private string? _name;
    private string? _firstName;
    private string? _secondName;
    private string? _email;
    private string? _password;

    [IsOptional]
    [MinLength(3, ErrorMessage = "El nombre es demasiado corto")]
    [MaxLength(191, ErrorMessage = "El nombre es demasiado largo")]
    //Verifica que hay solo letras y espacios
    [Regexp(@"^[áéíóúñÁÉÍÓÚÑa-zA-Z\s]+$", ErrorMessage = "Los nombres solo pueden tener letras")]
    // Verifica que los nombres tengan un formato como:
    // primerNombre segundoNombre
    // unicoNombre
    [Regexp(@"^\w+\s?\w+$", ErrorMessage = "Se permiten dos nombres maximo")]
    public string? Name {
      get => _name != null && _name.Trim() != ""
          ? Regex.Replace(_name.Trim(), @"(?<=\w+)\s{2,}(?=\w+)", " ")
          : null; // Trimea y reemplaza dos o mas espacios juntos a uno solo
      set => _name = value;
    }

    [IsOptional]
    [MinLength(3, ErrorMessage = "El apellido paterno es demasiado corto")]
    [MaxLength(191, ErrorMessage = "El apellido paterno es demasiado largo")]
    [Regexp(@"^[áéíóúñÁÉÍÓÚÑa-zA-Z]+", ErrorMessage = "El apellido paterno solo puede tener letras")]
    [Regexp(@"^\S+$", ErrorMessage = "El apellido paterno no puede tener espacios")]
    public string? FirstName {
      get => _firstName != null && _firstName.Trim() != "" ? _firstName.Trim() : null;
      set => _firstName = value;
    }

    [IsOptional]
    [MinLength(3, ErrorMessage = "El apellido materno es demasiado corto")]
    [MaxLength(191, ErrorMessage = "El apellido materno es demasiado largo")]
    [Regexp(@"^[áéíóúñÁÉÍÓÚÑa-zA-Z]+$", ErrorMessage = "El apellido materno solo puede tener letras")]
    [Regexp(@"^\S+$", ErrorMessage = "El apellido materno no puede tener espacios")]
    public string? SecondName {
      get => _secondName != null && _secondName.Trim() != "" ? _secondName.Trim() : null;
      set => _secondName = value;
    }

    [IsOptional]
    [EmailAddress(ErrorMessage = "El email proporcionado debe ser valido")]
    public string? Email {
      get => _email != null && _email.Trim() != "" ? _email.Trim() : null;
      set => _email = value;
    }

    [IsOptional]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos {1} caracteres")]
    [MaxLength(80, ErrorMessage = "La contraseña es demasiado larga")]
    public string? Password {
      get => _password != null && _password.Trim() != "" ? _password?.Trim() : null;
      set => _password = value;
    }

    [IsOptional]
    [Regexp("([1-9][0-9]*)", ErrorMessage = "El identificador del rol debe ser un número entero positivo")]
    public int? RoleId { get; set; }
  }
}
