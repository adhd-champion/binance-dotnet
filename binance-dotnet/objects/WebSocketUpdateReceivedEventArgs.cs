using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class WebSocketUpdateReceivedEventArgs : EventArgs
    {
        public WebSocketUpdateReceivedEventArgs(string message, bool connectionOpen)
        {
            Message = message;
            ConnectionOpen = connectionOpen;
            Timestamp = DateTime.Now;
        }
        public bool ConnectionOpen { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
