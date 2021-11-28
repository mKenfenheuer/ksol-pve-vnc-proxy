using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Aprox.Api.Model
{
    public class PveVncTicket
    {
        [JsonProperty("upid")]
        public string Upid { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        [JsonProperty("cert")]
        public string Cert { get; set; }

        [JsonConstructor]
        public PveVncTicket(string upid, string user, int port, string ticket, string cert)
        {
            Upid = upid;
            User = user;
            Port = port;
            Ticket = ticket;
            Cert = cert;
        }
    }
}
