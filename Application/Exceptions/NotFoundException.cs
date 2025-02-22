using System.Net;

namespace Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(List<string> errorMEssages = default, HttpStatusCode statusCode = HttpStatusCode.NotFound)
        {
            ErrorMessages = errorMEssages;
            StatusCode = statusCode;
        }

        public List<string> ErrorMessages { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
