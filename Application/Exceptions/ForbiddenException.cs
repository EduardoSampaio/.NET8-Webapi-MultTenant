using System.Net;

namespace Application.Exceptions
{
    public class  ForbiddenException: Exception
    {
        public ForbiddenException(List<string> errorMEssages = default, HttpStatusCode statusCode = HttpStatusCode.Forbidden)
        {
            ErrorMessages = errorMEssages;
            StatusCode = statusCode;
        }

        public List<string> ErrorMessages { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
