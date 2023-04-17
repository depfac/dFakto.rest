using dFakto.Rest.Abstractions;
using dFakto.Rest.SampleApi.Domain;

namespace dFakto.Rest.SampleApi.ResourceFactories;

public class BookResourceFactory
{
    private readonly IResourceFactory _resourceFactory;
    private readonly ResourceUriFactory _resourceUriFactory;

    public BookResourceFactory(IResourceFactory resourceFactory, ResourceUriFactory resourceUriFactory)
    {
        _resourceFactory = resourceFactory;
        _resourceUriFactory = resourceUriFactory;
    }
    
    public IResource GetBookResource(Book book)
    {
        var res = _resourceFactory.Create(_resourceUriFactory.GetBookUri(book.Isbn));
        res.AddLink("author", _resourceUriFactory.GetAuthorByNameUri(book.AuthorName));
        res.Add(new
        {
            book.Title,
            book.Isbn,
            book.Editor,
            Author = book.AuthorName
        });
        return res;
    }
    
    public IResource GetBooksResource(IReadOnlyList<Book> books)
    {
        var r = _resourceFactory.Create(_resourceUriFactory.GetBooksUri());
        r.AddEmbeddedResource("books", books.Select(GetBookResource));
        return r;
    }
    public IResource GetBooksResource(IReadOnlyList<Book> books, string authorName)
    {
        var r = _resourceFactory.Create(_resourceUriFactory.GetAuthorBooksUri(authorName));
        r.AddEmbeddedResource("books", books.Select(GetBookResource));
        return r;
    }
}