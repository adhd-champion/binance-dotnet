using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class Response_CancelOrder : APIResponse
    {
        public string symbol { get; set; }
        public string origClientOrderId { get; set; }
        public int orderId { get; set; }
        public string clientOrderId { get; set; }
    }

}
