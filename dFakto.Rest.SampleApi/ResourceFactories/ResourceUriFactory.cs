using dFakto.Rest.AspNetCore.Mvc;

namespace dFakto.Rest.SampleApi.ResourceFactories;

public class ResourceUriFactory
{   
    public const string GetAuthorBooks = "GetAuthorBooks";
    public const string GetAuthors = "GetAuthors";
    public const string GetBooks = "GetBooks";
    public const string GetBook = "GetBook";
    public const string GetAuthorName = "GetAuthorByName";
    
    private readonly IResourceUriFactory _resourceUriFactory;

    public ResourceUriFactory(IResourceUriFactory resourceUriFactory)
    {
        _resourceUriFactory = resourceUriFactory;
    }

    public Uri GetAuthorsUri()
    {
        return _resourceUriFactory.GetUriByName(GetAuthors);
    }
    
    
    public Uri GetBooksUri()
    {
        return _resourceUriFactory.GetUriByName(GetBooks);
    }
    
    public Uri GetAuthorByNameUri(string authorName)
    {
        return _resourceUriFactory.GetUriByName(GetAuthorName, new {Name = authorName});
    }
    
    public Uri GetAuthorBooksUri(string authorName)
    {
        return _resourceUriFactory.GetUriByName(GetAuthorBooks, new {Name = authorName});
    }

    public Uri GetBookUri(string bookIsbn)
    {
        return _resourceUriFactory.GetUriByName(GetBook, new {Isbn = bookIsbn});
    }
}