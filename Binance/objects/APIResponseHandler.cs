using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Script.Serialization;

namespace Binance.objects.api_responses
{
    public static class APIResponseHandler
    {
        public static T CreateAPIResponseObject<T>(string json, string arrayName = null) 
        {
            if (string.IsNullOrEmpty(arrayName))
                arrayName = "ArrayElement";
            if (json.Trim().StartsWith("["))
                json = "{ \"" + arrayName + "\": " + json;
            if (json.Trim().EndsWith("]"))
                json = json + " }";
            JavaScriptSerializer java = new JavaScriptSerializer();
            object obj;
            try
            {
                obj = java.Deserialize<T>(json);
            }
            catch(Exception ex)
            {
                obj=(T)Activator.CreateInstance(typeof(T));
                ((APIResponse)obj).code = ex.HResult;
                ((APIResponse)obj).msg = ex.Message;
            }
            ((APIResponse)obj).raw = json;
            return ((T)obj);
        }
    }
}
