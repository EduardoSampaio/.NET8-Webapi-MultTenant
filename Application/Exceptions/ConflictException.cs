using System.Net;

namespace Application.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(List<string> errorMEssages = default, HttpStatusCode statusCode = HttpStatusCode.Conflict)
        {
            ErrorMEssages = errorMEssages;
            StatusCode = statusCode;
        }

        public List<string> ErrorMEssages { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
