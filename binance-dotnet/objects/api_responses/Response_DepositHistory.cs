using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class Response_DepositHistory : APIResponse
    {
        public Depositlist[] depositList { get; set; }
        public bool success { get; set; }
    }

    public class Depositlist
    {
        public long insertTime { get; set; }
        public float amount { get; set; }
        public string asset { get; set; }
        public int status { get; set; }
    }
}
