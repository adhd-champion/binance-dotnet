using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class Response_UserDataStream : APIResponse
    {
        public string listenKey { get; set; }
    }
}
