using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Emerald.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static object TryParse(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject(str);
            }
            catch
            {
                return str;
            }
        }

        public static string ToJson(this object obj, Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(obj, formatting, JsonSerializerSettings);
    }
}