using dFakto.Rest.Abstractions;
using dFakto.Rest.AspNetCore.Mvc;
using dFakto.Rest.SampleApi.Domain;

namespace dFakto.Rest.SampleApi.ResourceFactories;

public class BookResourceFactory
{
    private readonly IResourceFactory _resourceFactory;
    private readonly LinksFactory _linkResourceFactory;

    public BookResourceFactory(IResourceFactory resourceFactory, LinksFactory linkResourceFactory)
    {
        _resourceFactory = resourceFactory;
        _linkResourceFactory = linkResourceFactory;
    }
    
    public IResource GetBookResource(Book book)
    {
        var res = _resourceFactory.Create(_linkResourceFactory.GetBookUri(book.Isbn));
        res.AddLink("author", _linkResourceFactory.GetAuthorByNameUri(book.AuthorName));
        res.Add(new
        {
            book.Title,
            book.Isbn,
            book.Editor,
            Author = book.AuthorName
        });
        return res;
    }
    
    public object? GetBooksResource(IReadOnlyList<Book> books)
    {
        var r = _resourceFactory.Create(_linkResourceFactory.GetBooksUri());
        r.AddEmbeddedResource("books", books.Select(GetBookResource));
        return r;
    }
    public object? GetBooksResource(IReadOnlyList<Book> books, string authorName)
    {
        var r = _resourceFactory.Create(_linkResourceFactory.GetAuthorBooksUri(authorName));
        r.AddEmbeddedResource("books", books.Select(GetBookResource));
        return r;
    }
}