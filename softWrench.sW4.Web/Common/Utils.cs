using Newtonsoft.Json;

namespace softWrench.sW4.Web.Common
{
    public static class Utils
    {

        public static string jsonString(object obj, string msg = "Success", bool Success = true)
        {
            return JsonConvert.SerializeObject(new { Result = obj, Msg = msg, Success = Success });
        }
    }
}