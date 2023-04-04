using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace dFakto.Rest.SampleApi.Domain;

public class AuthorsStore
{
    private readonly List<Author> _authors = new List<Author>();

    public AuthorsStore()
    {
        _authors.Add(new Author("bob",new DateTimeOffset(1959,1,23,0,0,0,TimeSpan.Zero)));
        _authors.Add(new Author("frank",new DateTimeOffset(2000,5,10,0,0,0,TimeSpan.Zero)));
        _authors.Add(new Author("robert",new DateTimeOffset(1845,7,1,0,0,0,TimeSpan.Zero)));
        _authors.Add(new Author("jules",new DateTimeOffset(1658,10,2,0,0,0,TimeSpan.Zero)));
    }
    
    public Author? GetByName(string name)
    {
        return _authors.FirstOrDefault(x => x.Name == name);
    }

    public IReadOnlyList<Author> GetAll()
    {
        return _authors;
    }
}