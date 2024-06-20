using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.Data.Exceptions {
  /// <summary>
  /// Excepcion creada con el proposito de devolver algo (mas que nada errores) en especifico al cliente 
  /// desde los repositorios (servicios) mediante el filtro de excepciones.
  /// </summary>
  public class HttpResponseException: Exception {

    public int StatusCode { get; set; }
    new public dynamic? Data { get; set; } = null;
    new public string? Message { get; set; } = null;

    public HttpResponseException(int statusCode) => StatusCode = statusCode;

    public HttpResponseException (int statusCode, string message) {
      StatusCode = statusCode;
      Message = message;
    }

    public HttpResponseException (int statusCode, dynamic data) {
      StatusCode = statusCode;
      Data = data;
    }

    public HttpResponseException (int statusCode, string message, dynamic data) {
      StatusCode = statusCode;
      Data = data;
      Message = message;
    }
  }
}
