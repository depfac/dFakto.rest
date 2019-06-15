using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dFakto.Rest
{
    public class ResourceConverter : JsonConverter
    {
        private const string Links = "_links";
        private const string Embedded = "_embedded";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resource = (Resource) value;
            writer.WriteStartObject();
            writer.WritePropertyName(Links);
            writer.WriteStartObject();
            foreach (var link in resource.Links)
            {
                writer.WritePropertyName(link.Key);
                if (link.Value.Count == 1)
                {
                    JObject.FromObject(link.Value[0], serializer).WriteTo(writer);
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var l in link.Value)
                    {
                        JObject.FromObject(l, serializer).WriteTo(writer);
                    }

                    writer.WriteEndArray();
                }
            }

            writer.WriteEndObject(); // End links

            if (resource.Embedded.Count > 0)
            {
                writer.WritePropertyName(Embedded);
                writer.WriteStartObject();
                foreach (var embedded in resource.Embedded)
                {
                    writer.WritePropertyName(embedded.Key);
                    if (embedded.Value.Count == 1)
                    {
                        JObject.FromObject(embedded.Value[0], serializer).WriteTo(writer);
                    }
                    else
                    {
                        writer.WriteStartArray();
                        foreach (var l in embedded.Value)
                        {
                            JObject.FromObject(l, serializer).WriteTo(writer);
                        }

                        writer.WriteEndArray();
                    }
                }

                writer.WriteEndObject(); // End embedded
            }

            foreach (var field in resource.Fields)
            {
                writer.WritePropertyName(field.Key);
                JToken.FromObject(field.Value).WriteTo(writer);
            }

            writer.WriteEndObject(); // End
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var o = JObject.Load(reader);

            var r = new Resource();
            foreach (var p in o)
            {
                switch (p.Key)
                {
                    case Links:
                        foreach (var links in (JObject) p.Value)
                        {
                            if (links.Value is JArray)
                            {
                                foreach (var link in (JArray) links.Value)
                                {
                                    r.AddLink(links.Key, link.ToObject<Link>());
                                }
                            }
                            else
                            {
                                r.AddLink(links.Key, links.Value.ToObject<Link>());
                            }
                        }

                        break;
                    case Embedded:
                        foreach (var embeddeds in (JObject) p.Value)
                        {
                            if (embeddeds.Value is JArray)
                            {
                                foreach (var embed in (JArray) embeddeds.Value)
                                {
                                    r.AddEmbedded(embeddeds.Key, embed.ToObject<Resource>());
                                }
                            }
                            else
                            {
                                r.AddEmbedded(embeddeds.Key, embeddeds.Value.ToObject<Resource>());
                            }
                        }

                        break;
                    default:
                        r.Add(p.Key, p.Value);
                        break;
                }
            }

            return r;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Resource);
        }
    }
}