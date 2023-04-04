using dFakto.Rest.Abstractions;
using dFakto.Rest.SampleApi.Domain;
using dFakto.Rest.SampleApi.ResourceFactories;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest.SampleApi.Controllers;

[ApiController]
[Route("api/")]
public class AuthorsController : Controller
{
    private readonly AuthorsStore _authorsStore;
    private readonly BooksStore _booksStore;
    private readonly AuthorResourceFactory _authorResourceFactory;
    private readonly BookResourceFactory _bookResourceFactory;

    public AuthorsController(
        AuthorsStore authorsStore,
        BooksStore booksStore,
        AuthorResourceFactory authorResourceFactory,
        BookResourceFactory bookResourceFactory)
    {
        _authorsStore = authorsStore;
        _booksStore = booksStore;
        _authorResourceFactory = authorResourceFactory;
        _bookResourceFactory = bookResourceFactory;
    }
    
    [HttpGet]
    [Route("authors", Name = LinksFactory.GetAuthors)]
    public ActionResult<IResource> GetAuthors()
    {
        var authors = _authorsStore.GetAll();
        return Ok(_authorResourceFactory.GetAuthorsResource(authors));
    }
    
    [HttpGet]
    [Route("authors/{name}", Name = LinksFactory.GetAuthorName)]
    public ActionResult<IResource> GetAuthor(string name)
    {
        var authors = _authorsStore.GetByName(name);
        return Ok(_authorResourceFactory.GetAuthorResource(authors));
    }
}