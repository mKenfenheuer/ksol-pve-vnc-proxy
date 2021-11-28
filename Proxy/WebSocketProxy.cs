using Aprox.Api;
using Aprox.Api.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace KSol_PVE_VNC_Proxy.Proxy
{
    public class WebSocketProxy
    {
        private ClientWebSocket _webSocket;
        private PveResource _pveResource;
        private PveVncTicket _vncTicket;
        private ProxmoxApi _api;
        private PveTicketData _ticket;
        private Task ProxyTask;

        public bool Active { get; private set; }
        public Action<WebSocketProxy> OnProxyStop { get; set; }
        public int Port { get; private set; }

        public WebSocketProxy(PveTicketData ticket, PveResource pveResource, PveVncTicket vncTicket, ProxmoxApi api, int port)
        {
            _vncTicket = vncTicket;
            _api = api;
            Port = port;
            _pveResource = pveResource;
            _ticket = ticket;
        }

        private async Task DoProxy()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, Port);
            try
            {
                tcpListener.Start();
                TcpClient client = await tcpListener.AcceptTcpClientAsync();

                Stream clientStream = client.GetStream();

                Active = true;

                var SendTask = Task.Run(async () =>
                {
                    ArraySegment<byte> buffer = new(new byte[1024]);
                    while (Active)
                    {
                        var result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        if (result.Count > 0)
                            await clientStream.WriteAsync(buffer.Array, 0, result.Count);
                    }
                });

                var ReceiveTask = Task.Run(async () =>
                {
                    byte[] buffer = new byte[1024];
                    while (Active)
                    {
                        var length = await clientStream.ReadAsync(buffer, 0, buffer.Length);
                        if (length > 0)
                            await _webSocket.SendAsync(new ArraySegment<byte>(buffer.Take(length).ToArray()), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                });

                await Task.WhenAny(new Task[] { SendTask, ReceiveTask });
                Active = false;
            } catch(Exception ex)
            {

            }
            tcpListener.Stop();
            await Task.Run(() => OnProxyStop(this));
        }

        public async Task StartProxy()
        {
            Uri url = _api.GetVncWebSocketConsoleUrl(_pveResource, _vncTicket);
            _webSocket = new ClientWebSocket();
            _webSocket.Options.RemoteCertificateValidationCallback = (caller, cert, chain, errors) =>
            {
                SHA1 sHA1 = SHA1.Create();
                var hash = sHA1.ComputeHash(cert.GetRawCertData());
                string hexString = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return hexString.Equals(_api.ServerThumbprint) || _api.ServerThumbprint == null;
            };
            var container = new CookieContainer();
            container.Add(url, new Cookie("PVEAuthCookie", _ticket.Ticket));
            _webSocket.Options.Cookies = container;
            _webSocket.Options.SetRequestHeader("CSRFPreventionToken", _ticket.CSRFPreventionToken);

            await _webSocket.ConnectAsync(url, CancellationToken.None);

            ProxyTask = DoProxy();
        }
    }
}
