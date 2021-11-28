using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Aprox.Api.Model
{
    internal class HttpApiResponse<T>
    {
        [JsonProperty("error")]
        public string Error { get; private set; }
        [JsonProperty("data")]
        public T Data { get; private set; }

        [JsonConstructor]
        public HttpApiResponse(string error, T data)
        {
            Error = error;
            Data = data;
        }
    }
}
