using Aprox.Api.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace Aprox.Api
{
    public class ProxmoxApi
    {
        private string _serverUrl = "";
        public string ServerUrl { get => _serverUrl; set => _serverUrl = value.Trim('/'); }
        public string ServerThumbprint { get; set; }

        private HttpClient HttpClient { get; set; } 

        public ProxmoxApi(IConfiguration configuration)
        {
            HttpClient = new HttpClient(new System.Net.Http.HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (o, cert, chain, errors) =>
                {
                    if (ServerThumbprint == null)
                        ServerThumbprint = cert.Thumbprint.ToLower();
                    
                    return cert.Thumbprint.ToLower().Equals(ServerThumbprint);
                }
            });

            var section = configuration.GetSection("ProxmoxApi");

            ServerUrl = section["Url"].ToString();
            ServerThumbprint = section["Thumbprint"]?.ToLower();
        }

        public async Task<ApiResponse<PveVncTicket>> GetVncTicketAsync(PveResource resource, PveTicketData ticket)
        {
            HttpClient.DefaultRequestHeaders.Add("CSRFPreventionToken", ticket.CSRFPreventionToken);
            HttpClient.DefaultRequestHeaders.Add("Cookie",$"PVEAuthCookie={ticket.Ticket}");

            string endpoint = "";
            if (resource.Type == "node")
            {
                endpoint = $"/api2/json/nodes/{resource.Node}/vncproxy";
            }
            else
            {
                endpoint = $"/api2/json/nodes/{resource.Node}/{resource.Type}/{resource.Id}/vncproxy";
            }


            var data = new Dictionary<string, string>()
            {
                { "websocket" , "1" },
            };

            return await MakeApiPostRequestAsync<PveVncTicket>(endpoint, new FormUrlEncodedContent(data));
        }

        public Uri GetVncWebSocketConsoleUrl(PveResource resource, PveVncTicket ticket)
        {
            if (resource.Type == "node")
            {
                return new Uri($"{ServerUrl.Replace("http","ws")}/api2/json/nodes/{resource.Node}/vncwebsocket?port={ticket.Port}&vncticket={ticket.Ticket}");
            }
            else 
            {
                return new Uri($"{ServerUrl.Replace("http", "ws")}/api2/json/nodes/{resource.Node}/{resource.Type}/{resource.Id}/vncwebsocket?port={ticket.Port}&vncticket={HttpUtility.UrlEncode(ticket.Ticket)}");
            }
        }
        #region ApiInternals
        private async Task<ApiResponse<T>> MakeApiPostRequestAsync<T>(string path, HttpContent content)
        {
            HttpResponseMessage response = await HttpClient.PostAsync($"{ServerUrl}{path}", content);
            var payload = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                HttpApiResponse<T> apiResponse = JsonConvert.DeserializeObject<HttpApiResponse<T>>(payload);
                return new ApiResponse<T>((int)response.StatusCode, null, apiResponse.Data, response.IsSuccessStatusCode);
            }
            else
            {
                return new ApiResponse<T>((int)response.StatusCode, response.ReasonPhrase + " " + payload, default, response.IsSuccessStatusCode);
            }
        }
        #endregion
    }
}
