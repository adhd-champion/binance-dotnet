using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.objects.api_responses
{
    public class Response_NewOrder : APIResponse
    {
        public string symbol { get; set; }
        public int orderId { get; set; }
        public string clientOrderId { get; set; }
        public long transactTime { get; set; }
    }
}
