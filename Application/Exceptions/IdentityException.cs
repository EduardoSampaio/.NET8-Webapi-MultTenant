using System.Net;

namespace Application.Exceptions
{
    public class IdentityException : Exception
    {
        public IdentityException(List<string> errorMEssages = default, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            ErrorMessages = errorMEssages;
            StatusCode = statusCode;
        }

        public List<string> ErrorMessages { get; set; }
        public HttpStatusCode StatusCode { get; set; }

    }
