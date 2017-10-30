using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using binance_dotnet.enums;
namespace binance_dotnet.objects
{
    public class WebSocketUpdateReceivedEventArgs : EventArgs
    {
        public WebSocketUpdateReceivedEventArgs(string message, bool connectionOpen, WebSocketUpdateTypes type)
        {
            Message = message;
            ConnectionOpen = connectionOpen;
            Timestamp = DateTime.Now;
            UpdateType = type;
        }
        public bool ConnectionOpen { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }
        public WebSocketUpdateTypes UpdateType { get; private set; }
    }
}
