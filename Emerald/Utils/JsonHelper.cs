using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Emerald.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static string Serialize(object obj, Formatting formatting = Formatting.Indented)
        {
            return JsonConvert.SerializeObject(obj, formatting, JsonSerializerSettings);
        }
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        public static object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static string ToJson(this object obj, Formatting formatting = Formatting.None) => Serialize(obj, formatting);
        public static T ParseJson<T>(this string str) => JsonConvert.DeserializeObject<T>(str);
        public static object ParseJson(this string str, Type type) => JsonConvert.DeserializeObject(str, type);
    }
}