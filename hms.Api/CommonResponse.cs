using System.Net;

namespace hms.Api
{
    public class CommonResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public object Data { get; set; }
    }
}