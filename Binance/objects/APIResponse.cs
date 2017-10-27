using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Script.Serialization;

namespace Binance.objects
{
    public class APIResponse
    {
        private JavaScriptSerializer java = new JavaScriptSerializer();
        public int? code { get; set; }
        public string msg { get; set; }
        public string raw { get; set; }

        public bool hasErrors
        {
            get
            {
                if (code == null && string.IsNullOrEmpty(msg))
                    return false;
                else
                    return true;
            }
        }
        public string status
        {
            get
            {
                if (!hasErrors)
                    return "Succeeded";
                else
                    return "Failed";
            }
        }
    }
}
