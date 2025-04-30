using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Sdk.Runtime.JsonSerializable
{
    public class PrivateResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var properties = type.GetProperties(bindingFlags);
            var fields = type.GetFields(bindingFlags);

            var jsonProperties = new List<JsonProperty>();

            // // 处理属性
            // foreach (var property in properties)
            // {
            //     if (property.GetCustomAttribute<JsonExcludeAttribute>() != null)
            //     {
            //         continue;
            //     }
            //
            //     var jsonProperty = base.CreateProperty(property, memberSerialization);
            //     jsonProperty.Writable = true;
            //     jsonProperty.Readable = true;
            //     jsonProperties.Add(jsonProperty);
            // }

            // 处理字段
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<JsonExcludeAttribute>() != null)
                {
                    continue;
                }

                var jsonProperty = base.CreateProperty(field, memberSerialization);
                jsonProperty.Writable = true;
                jsonProperty.Readable = true;
                jsonProperties.Add(jsonProperty);
            }

            return jsonProperties;
        }
    }
}