using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace FluentGraphQL
{
    internal static class JObjectExtensions
    {
        public static JObject CleanObject(this JObject jobject)
        {
            var properties = jobject
                .Properties()
                .ToList();

            foreach (var property in properties)
            {
                if (property.Value is JObject)
                {
                    CleanObject((JObject)property.Value);

                    continue;
                }

                if (property.Value is JArray)
                {
                    foreach (var value in property.Value)
                    {
                        if (value.Type == JTokenType.Object)
                        {
                            CleanObject(value as JObject);
                        }

                        if (value.Type == JTokenType.Date)
                        {
                            if (value.Value<DateTime>() == new DateTime())
                            {
                                jobject.Remove(property.Name);
                            }
                        }
                    }

                    continue;
                }

                if (property.Value.Type == JTokenType.Date)
                {
                    if (property.Value.Value<DateTime>() == new DateTime())
                    {
                        property.Value = JToken.FromObject(DateTime.Now);
                    }
                }
            }

            return jobject;
        }
    }
}
