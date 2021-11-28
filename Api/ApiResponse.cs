using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aprox.Api.Model
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public T ResponsePayload { get; private set; }
        public bool IsSuccess { get; private set; }

        [JsonConstructor]
        public ApiResponse(int statusCode, string errorMessage, T responsePayload, bool isSuccess)
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ResponsePayload = responsePayload;
            IsSuccess = isSuccess;
        }
    }
}
