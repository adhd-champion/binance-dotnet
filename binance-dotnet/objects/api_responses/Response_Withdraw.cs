using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class Response_Withdraw : APIResponse
    {
        public new string msg { get; set; }
        public bool success { get; set; }
    }

}
