using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AdaptiveCards
{
    class AdaptiveInlinesConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(List<IAdaptiveInline>).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            List<object> list = array.ToObject<List<object>>();
            List<IAdaptiveInline> arrayList = new List<IAdaptiveInline>();

            // We only support text runs for now, which can be specified as either a string or an object
            foreach (object obj in list)
            {
                if (obj is string s)
                {
                    arrayList.Add(new AdaptiveTextRun(s));
                }
                else
                {
                    JObject jobj = (JObject)obj;
                    arrayList.Add((IAdaptiveInline)jobj.ToObject(typeof(AdaptiveTextRun)));
                }
            }
            return arrayList;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
