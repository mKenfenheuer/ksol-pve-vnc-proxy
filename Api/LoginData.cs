using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Aprox.Api.Model
{
    public class PveTicketData
    {
        public PveTicketData(HttpRequest request)
        {
            CSRFPreventionToken = request.Headers["CSRFPreventionToken"];
            Ticket = request.Cookies["PVEAuthCookie"];
        }

        [JsonProperty("CSRFPreventionToken")]
        public string CSRFPreventionToken { get; private set; }
        [JsonProperty("ticket")]
        public string Ticket { get; set; }
    }
}
