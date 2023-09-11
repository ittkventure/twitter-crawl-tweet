using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System
{
    public static class JsonHelper
    {

        public static JObject ToJObject(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return JObject.FromObject(obj);
        }

        public static JToken ToJToken(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return JToken.FromObject(obj);
        }

        public static string SelectStringToken(this JObject obj, string key)
        {
            var o = obj.SelectToken(key);
            return (o == null) ? "" : (string)o;
        }

        public static bool SelectBoolToken(this JObject obj, string key)
        {
            var o = obj.SelectToken(key);
            return (o == null) ? false : (bool)o;
        }

        /// <summary>
        /// Chuyển json object sang kiểu chỉ định
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jObj"></param>
        /// <returns></returns>
        public static T ConvertJObject<T>(this object jObj)
        {
            if (jObj is JObject)
            {
                var result = (jObj as JObject).ToObject<T>();
                return result;
            }
            if (jObj is JArray)
            {
                var result = (jObj as JArray).ToObject<T>();
                return result;
            }
            return default(T);
        }

        public static object ConvertJObject(this object jObj, Type t)
        {
            if (jObj is JObject)
            {
                var result = (jObj as JObject).ToObject(t);
                return result;
            }
            if (jObj is JArray)
            {
                var result = (jObj as JArray).ToObject(t);
                return result;
            }
            return null;
        }

        public static string Stringify(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T Parse<T>(string json)
        {
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public static T TryParse<T>(string json)
        {
            try
            {
                return Parse<T>(json);
            }
            catch
            {
                return default(T);
            }
        }

        public static object Parse(string json, Type type)
        {
            var obj = JsonConvert.DeserializeObject(json, type);
            return obj;
        }

        public static object TryParse(string json, Type type)
        {
            try
            {
                return Parse(json, type);
            }
            catch
            {
                return default;
            }
        }

        public static T ParseIfNotNull<T>(this JToken token)
        {
            if (token == null)
            {
                return default(T);
            }

            return token.Value<T>();
        }
    }
}
