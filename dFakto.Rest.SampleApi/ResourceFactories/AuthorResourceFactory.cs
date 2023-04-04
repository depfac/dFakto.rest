using dFakto.Rest.Abstractions;
using dFakto.Rest.SampleApi.Domain;

namespace dFakto.Rest.SampleApi.ResourceFactories;

public class AuthorResourceFactory
{
    private readonly IResourceFactory _resourceFactory;
    private readonly LinksFactory _linkResourceFactory;

    public AuthorResourceFactory(IResourceFactory resourceFactory, LinksFactory linkResourceFactory)
    {
        _resourceFactory = resourceFactory;
        _linkResourceFactory = linkResourceFactory;
    }

    public IResource GetAuthorResource(Author author)
    {
        var res = _resourceFactory.Create(_linkResourceFactory.GetAuthorByNameUri(author.Name));
        res.AddLink("books", _linkResourceFactory.GetAuthorBooksUri(author.Name));
        res.Add(author);
        return res;
    }

    public IResource GetAuthorsResource(IReadOnlyList<Author> authors)
    {
        var r = _resourceFactory.Create(_linkResourceFactory.GetAuthorsUri());
        r.AddEmbeddedResource("authors", authors.Select(GetAuthorResource));
        return r;
    }
}