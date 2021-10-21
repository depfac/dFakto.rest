using System.Collections.Generic;
using System.Linq;
using dFakto.Rest.Abstractions;
using dFakto.Rest.AspNetCore.Mvc;
using dFakto.Rest.SampleApi.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace dFakto.Rest.SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : Controller
    {
        private readonly IResourceFactory _resourceFactory;
        private readonly SampleRepository _repository;

        public ValuesController(IResourceFactory resourceFactory, SampleRepository repository)
        {
            _resourceFactory = resourceFactory;
            _repository = repository;
        }

        private IResource GetSampleResource(MySampleValue sampleValue)
        {
            return _resourceFactory.Create(Url.LinkUri("getbyid", new {id = sampleValue.Id}))
                .AddLink("somelink", new Link(Url.LinkUri("getallvalues")))
                .Add(sampleValue);
        }

        private IResource GetSampleResourceCollection(
            ResourceCollectionRequest request,
            IEnumerable<MySampleValue> values)
        {
            var r = _resourceFactory.Create(Url.LinkUri("getallvalues"))
                .Add(request)
                .AddEmbedded("values", values.Select(x => GetSampleResource(x)));
            return r;
        }

        // GET api/values
        [HttpGet()]
        [Route("",Name="getallvalues")]
        public ActionResult<IResource> Get([FromQuery] ResourceCollectionRequest request)
        {
            return Ok(GetSampleResourceCollection(request,_repository.List(new CollectionRequestSpecification<MySampleValue>(request))));
        }

        [HttpOptions]
        [Route("")]
        public ActionResult Options()
        {
            Response.Headers.Add(HeaderNames.Accept,new StringValues("GET,POST,DELETE"));
            return Ok();
        }

        // GET api/values/5
        [HttpGet("{id}",Name = "getbyid")]
        public ActionResult<IResource> Get(int id, [FromQuery] ResourceRequest request)
        {
            return Ok(GetSampleResource(_repository.GetById(id)));
        }

        // POST api/values
        [HttpPost]
        public ActionResult Post(MySampleValue resource)
        {
            var r = GetSampleResource(_repository.Create(resource));
            return Created(r.Self, r);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public OkResult Put(int id, [FromBody] MySampleValue value)
        {
            _repository.Update(id, value);
            return Ok();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public OkResult Delete(int id)
        {
            _repository.DeleteById(id);
            return Ok();
        }
    }
}