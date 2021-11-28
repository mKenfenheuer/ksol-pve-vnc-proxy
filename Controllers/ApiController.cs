using Aprox.Api;
using Aprox.Api.Model;
using KSol_PVE_VNC_Proxy.Proxy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KSol_PVE_VNC_Proxy.Controllers
{
    [ApiController]
    [Route("/")]
    public class ApiController : ControllerBase
    {
        private WebSocketProxyManager _proxyManager;
        private ProxmoxApi _api;
        private ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger, ProxmoxApi api, WebSocketProxyManager proxyManager)
        {
            _logger = logger;
            _api = api;
            _proxyManager = proxyManager;
        }

        [HttpGet("/api2/json/nodes/{node}/{type}/{id}/plainvncproxy")]
        public async Task<ApiResponse<PveVncTicket>> StartPlainProxy([FromRoute] string node, [FromRoute] string type, [FromRoute] string id)
        {
            _logger.LogInformation($"Requesting VNC Ticket for {node}/{type}/{id}.");
            PveTicketData data = new PveTicketData(Request);
            PveResource resource = new PveResource(id, node, type);
            var ticketResponse = await _api.GetVncTicketAsync(resource, data);

            if (!ticketResponse.IsSuccess)
            {
                _logger.LogWarning($"Requesting VNC Ticket for {node}/{type}/{id} failed: {ticketResponse.ErrorMessage}");
                return ticketResponse;
            }
            try
            {
                _logger.LogInformation($"Successfully requested VNC Ticket for {node}/{type}/{id}.");
                _logger.LogInformation($"Starting vnc proxy.");
                var proxy = await _proxyManager.StartWebSocketProxy(resource, ticketResponse.ResponsePayload, data);
                _logger.LogInformation($"Started vnc proxy on port {proxy.Port}.");
                ticketResponse.ResponsePayload.Port = proxy.Port;
                return ticketResponse;
            } catch(Exception ex)
            {
                _logger.LogError($"Failed to start vnc proxy: {ex}");
                return new ApiResponse<PveVncTicket>(500, ex.Message, null, false);
            }

        }
    }
}
