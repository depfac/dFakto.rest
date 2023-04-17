using dFakto.Rest.Abstractions;
using dFakto.Rest.SampleApi.Domain;

namespace dFakto.Rest.SampleApi.ResourceFactories;

public class AuthorResourceFactory
{
    private readonly IResourceFactory _resourceFactory;
    private readonly ResourceUriFactory _resourceUriFactory;

    public AuthorResourceFactory(IResourceFactory resourceFactory, ResourceUriFactory resourceUriFactory)
    {
        _resourceFactory = resourceFactory;
        _resourceUriFactory = resourceUriFactory;
    }

    public IResource GetAuthorResource(Author author)
    {
        var res = _resourceFactory.Create(_resourceUriFactory.GetAuthorByNameUri(author.Name));
        res.AddLink("books", _resourceUriFactory.GetAuthorBooksUri(author.Name));
        res.Add(author);
        return res;
    }

    public IResource GetAuthorsResource(IReadOnlyList<Author> authors)
    {
        var r = _resourceFactory.Create(_resourceUriFactory.GetAuthorsUri());
        r.AddEmbeddedResource("authors", authors.Select(GetAuthorResource));
        return r;
    }
}