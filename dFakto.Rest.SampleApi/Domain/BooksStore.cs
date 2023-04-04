namespace dFakto.Rest.SampleApi.Domain;

public class BooksStore
{
    private readonly List<Book> _books = new List<Book>();

    public BooksStore()
    {
        _books.Add(new Book("aaaa","123456","bob","frank"));
        _books.Add(new Book("bbbb","456789","alice","frank"));
        _books.Add(new Book("cccc","134679","bob","robert"));
        _books.Add(new Book("dddd","124578","jules","frank"));
        _books.Add(new Book("eeee","235689","robert","frank"));
    }
    

    public Book? GetByIsbn(string isbn)
    {
        return _books.FirstOrDefault(x => x.Isbn == isbn);
    }

    public IReadOnlyList<Book> GetAll()
    {
        return _books;
    }

    public IReadOnlyList<Book> GetByAuthor(string authorName)
    {
        return _books.Where(x => x.AuthorName == authorName).ToArray();
    }
}