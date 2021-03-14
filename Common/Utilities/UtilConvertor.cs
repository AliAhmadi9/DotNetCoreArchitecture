using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Common.Utilities
{
    public static class UtilConvertor
    {
        public static T ToObject<T>([NotNull] dynamic input) where T : IBaseDTO
        {
            var obj = (JObject)input;
            if (obj == null || !obj.HasValues)
                return default;

            return (obj).ToObject<T>();
        }

        public static T ToObject<T>([NotNull] string input) where T : IBaseDTO
        {
            var obj = JsonConvert.DeserializeObject<dynamic>(input);
            if (obj == null || !obj.HasValues)
                return default;

            return ((JObject)obj).ToObject<T>();
        }

        public static List<string> GetPropertiesName([NotNull] dynamic input)
        {
            var obj = (JObject)input;
            if (obj == null || !obj.HasValues || !obj.Properties().Any())
                return null;

            return obj.Properties().Select(p => p.Name).ToList();
        }
        public static List<string> GetPropertiesName([NotNull] string input)
        {
            var obj = (JObject)JsonConvert.DeserializeObject<dynamic>(input);
            if (obj == null || !obj.HasValues || !obj.Properties().Any())
                return null;

            return obj.Properties().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool ContainProperty([NotNull] string input, [NotNull] string propertyName)
        {
            var obj = (JObject)JsonConvert.DeserializeObject<dynamic>(input);
            if (obj == null || !obj.HasValues || !obj.Properties().Any())
                return false;

            return obj.Properties().Any(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ContainProperty([NotNull] dynamic input, [NotNull] string propertyName)
        {
            var obj = (JObject)input;
            if (obj == null || !obj.HasValues || !obj.Properties().Any())
                return false;

            return obj.Properties().Any(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
