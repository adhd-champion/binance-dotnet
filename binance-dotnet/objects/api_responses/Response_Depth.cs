using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class Response_Depth : APIResponse
    {
        public int lastUpdateId { get; set; }
        public object[][] bids { get; set; }
        public object[][] asks { get; set; }
    }
}
