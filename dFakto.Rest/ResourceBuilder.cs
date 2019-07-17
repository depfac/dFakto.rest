using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace dFakto.Rest
{
    public class ResourceBuilder
    {
        private readonly JsonSerializer _serializer;

        public ResourceBuilder()
            :this(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            })
        {
        }

        public ResourceBuilder(JsonSerializerSettings settings)
            :this(JsonSerializer.Create(settings))
        {
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
            if(obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            return new Resource(obj,_serializer );
        }
        
        public async Task<Resource> LoadAsync(Stream stream)
        {
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new Resource(await JObject.LoadAsync(new JsonTextReader(new StreamReader(stream))), _serializer);
        }
        
        public async Task<Resource> LoadAsync(JsonReader reader)
        {
            if(reader == null)
                throw new ArgumentNullException(nameof(reader));

            return new Resource(await JObject.LoadAsync(reader), _serializer);
        }
    }
}