﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Emerald.Utils
{
    public static class ObjectExtension
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }
}