using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using binance_dotnet.enums;

namespace binance_dotnet.objects
{
    public class BinanceWebSocketEndpoint
    {
        public BinanceWebSocketEndpoint(WebSocketEndpoints name, string url)
        {
            Name = name;
            Url = url;
            CancelFlag = false;
            ChunksReceived = 0;
        }
        public WebSocketEndpoints Name { get; private set; }
        public string Url { get; private set; }
        public bool CancelFlag { get; private set; }
        internal void Close()
        {
            CancelFlag = true;
        }
        public int ChunksReceived { get; internal set; }
    }
}
