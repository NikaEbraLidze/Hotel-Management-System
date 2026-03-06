using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace hms.Api
{
    public class CommonResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Data { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string> Errors { get; set; }

        public static CommonResponse Success(
            object data,
            string message = "Success",
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new CommonResponse
            {
                IsSuccess = true,
                Message = message,
                StatusCode = statusCode,
                Data = data
            };
        }

        public static CommonResponse Fail(IEnumerable<string> errors, string message, HttpStatusCode statusCode)
        {
            return new CommonResponse
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors
            };
        }
    }
}
