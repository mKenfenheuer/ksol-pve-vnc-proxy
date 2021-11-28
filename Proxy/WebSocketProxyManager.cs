using Aprox.Api;
using Aprox.Api.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KSol_PVE_VNC_Proxy.Proxy
{
    public class WebSocketProxyManager
    {
        private List<WebSocketProxy> proxies = new List<WebSocketProxy>();
        private int PortRangeStart = 5900;
        private int PortRangeEnd = 5999;
        private ProxmoxApi _api;

        public WebSocketProxyManager(IConfiguration configuration, ProxmoxApi api)
        {
            var section = configuration.GetSection("Proxy");
            section.Bind(this);
            _api = api;
        }

        private void OnProxyStop(WebSocketProxy proxy)
        {
            lock (proxies)
                proxies.Remove(proxy);
        }

        public int GetNextFreePort()
        {
            return Enumerable.Range(PortRangeStart, PortRangeEnd - PortRangeStart - 1).First(p => !proxies.Select(p => p.Port).Contains(p));
        }

        public bool SlotAvailable()
        {
            return proxies.Count() < PortRangeEnd - PortRangeStart;
        }

        public async Task<WebSocketProxy> StartWebSocketProxy(PveResource resource, PveVncTicket vncTicket, PveTicketData ticketData)
        {
            if (!SlotAvailable())
                throw new Exception("There are no more free proxy slots available!");
            WebSocketProxy proxy;
            lock (proxies)
            {
                proxy = new WebSocketProxy(ticketData, resource, vncTicket, _api, GetNextFreePort());
                proxies.Add(proxy);
                proxy.OnProxyStop = this.OnProxyStop;
            }
            await proxy.StartProxy();
            return proxy;
        }
    }
}
