using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.objects.api_responses
{
    public class Response_Time : APIResponse
    {
        public long serverTime { get; set; }
    }
}
