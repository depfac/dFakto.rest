using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace dFakto.Rest
{
    public class ResourceConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ((Resource)value).Json.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            ResourceBuilder builder = new ResourceBuilder(serializer);
            return builder.Create((JObject.Load(reader)));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Resource);
        }
    }
}