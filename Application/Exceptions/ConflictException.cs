using System.Net;

namespace Application.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(List<string> errorMEssages = default, HttpStatusCode statusCode = HttpStatusCode.Conflict)
        {
            ErrorMessages = errorMEssages;
            StatusCode = statusCode;
        }

        public List<string> ErrorMessages { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
