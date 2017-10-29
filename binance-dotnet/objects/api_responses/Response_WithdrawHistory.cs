using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class Response_WithdrawHistory : APIResponse
    {
        public Withdrawlist[] withdrawList { get; set; }
        public bool success { get; set; }
    }

    public class Withdrawlist
    {
        public float amount { get; set; }
        public string address { get; set; }
        public string asset { get; set; }
        public long applyTime { get; set; }
        public int status { get; set; }
        public string txId { get; set; }
    }

}
