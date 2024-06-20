using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sigecendis_api.Data.Exceptions;
using System.Dynamic;

namespace sigecendis_api.Data.Filters {
  public class ExceptionFilter : IExceptionFilter {
    public void OnException(ExceptionContext context){

      int statusCode = 500;
      string message = null;
      object data = null;

      if (context.Exception is HttpResponseException) {
        message = ((HttpResponseException) context.Exception).Message;
        statusCode = ((HttpResponseException) context.Exception).StatusCode;
        data = ((HttpResponseException) context.Exception).Data;
      }

      if (message == null && statusCode == 500) message = "Ocurrio un problema en el servidor";

      dynamic values = new ExpandoObject();
      values.status = statusCode;

      if (message != null) values.message = message;
      if (data != null) values.data = data;

      context.Result = new ObjectResult(values) {
        StatusCode = statusCode
      };

      context.ExceptionHandled = true;
    }
  }
}
