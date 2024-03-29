using dFakto.Rest.Abstractions;
using dFakto.Rest.SampleApi.Domain;
using dFakto.Rest.SampleApi.ResourceFactories;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest.SampleApi.Controllers;

[ApiController]
[Route("api/")]
public class BooksController : Controller
{
    private readonly BooksStore _booksStore;
    private readonly BookResourceFactory _bookResourceFactory;

    public BooksController(
        BooksStore booksStore,
        BookResourceFactory bookResourceFactory)
    {
        _booksStore = booksStore;
        _bookResourceFactory = bookResourceFactory;
    }
    
    [HttpGet]
    [Route("books", Name = ResourceUriFactory.GetBooks)]
    public ActionResult<IResource> GetAuthors()
    {
        var books = _booksStore.GetAll();
        return Ok(_bookResourceFactory.GetBooksResource(books));
    }
    
    [HttpGet]
    [Route("books/{isbn}", Name = ResourceUriFactory.GetBook)]
    public ActionResult<IResource> GetAuthor(string isbn)
    {
        var book = _booksStore.GetByIsbn(isbn);
        if (book == null)
            return NotFound();
        return Ok(_bookResourceFactory.GetBookResource(book));
    }
    
    [HttpGet]
    [Route("authors/{name}/books", Name = ResourceUriFactory.GetAuthorBooks)]
    public ActionResult<IResource> GetAuthorBooks(string name)
    {
        var books = _booksStore.GetByAuthor(name);
        return Ok(_bookResourceFactory.GetBooksResource(books,name));
    }
}