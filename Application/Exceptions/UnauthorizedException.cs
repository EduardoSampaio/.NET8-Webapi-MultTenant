using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(List<string> errorMEssages = default, HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
        {
            ErrorMessages = errorMEssages;
            StatusCOde = statusCode;
        }

        public List<string> ErrorMessages { get; set; }
        public HttpStatusCode  StatusCOde { get; set; }
    }
}
