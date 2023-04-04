using dFakto.Rest.AspNetCore.Mvc;

namespace dFakto.Rest.SampleApi.ResourceFactories;

public class LinksFactory
{   
    public const string GetAuthorBooks = "GetAuthorBooks";
    public const string GetAuthors = "GetAuthors";
    public const string GetBooks = "GetBooks";
    public const string GetBook = "GetBook";
    public const string GetAuthorName = "GetAuthorByName";
    
    private readonly ILinkResourceFactory _linkResourceFactory;

    public LinksFactory(ILinkResourceFactory linkResourceFactory)
    {
        _linkResourceFactory = linkResourceFactory;
    }

    public Uri GetAuthorsUri()
    {
        return _linkResourceFactory.GetUriByName(GetAuthors);
    }
    
    
    public Uri GetBooksUri()
    {
        return _linkResourceFactory.GetUriByName(GetAuthors);
    }
    
    public Uri GetAuthorByNameUri(string authorName)
    {
        return _linkResourceFactory.GetUriByName(GetAuthorName, new {Name = authorName});
    }
    
    public Uri GetAuthorBooksUri(string authorName)
    {
        return _linkResourceFactory.GetUriByName(GetAuthorBooks, new {Name = authorName});
    }

    public Uri GetBookUri(string bookIsbn)
    {
        return _linkResourceFactory.GetUriByName(GetBook, new {Isbn = bookIsbn});
    }
}