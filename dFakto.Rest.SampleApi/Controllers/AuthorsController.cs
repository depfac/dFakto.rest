using dFakto.Rest.Abstractions;
using dFakto.Rest.SampleApi.Domain;
using dFakto.Rest.SampleApi.ResourceFactories;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest.SampleApi.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : Controller
{
    private readonly AuthorsStore _authorsStore;
    private readonly AuthorResourceFactory _authorResourceFactory;

    public AuthorsController(
        AuthorsStore authorsStore,
        AuthorResourceFactory authorResourceFactory)
    {
        _authorsStore = authorsStore;
        _authorResourceFactory = authorResourceFactory;
    }
    
    [HttpGet]
    [Route("", Name = LinksFactory.GetAuthors)]
    public ActionResult<IResource> GetAuthors()
    {
        var authors = _authorsStore.GetAll();
        return Ok(_authorResourceFactory.GetAuthorsResource(authors));
    }
    
    [HttpGet]
    [Route("{name}", Name = LinksFactory.GetAuthorName)]
    public ActionResult<IResource> GetAuthor(string name)
    {
        var author = _authorsStore.GetByName(name);
        if (author == null)
            return NotFound();
        
        return Ok(_authorResourceFactory.GetAuthorResource(author));
    }
}