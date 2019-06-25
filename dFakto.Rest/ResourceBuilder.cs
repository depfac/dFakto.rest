using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dFakto.Rest
{
    public class ResourceBuilder
    {
        private readonly JsonSerializer _serializer;

        public ResourceBuilder(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }

        public ResourceBuilder(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public Resource Create()
        {
            return Create(new JObject());
        }

        public Resource Create(JObject obj)
        {
            return new Resource(obj,_serializer );
        }
    }
}