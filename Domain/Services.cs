using System.Collections.Concurrent;
using Domain.Abstractions;
using Domain.Models;
using System.Threading;

namespace Domain.Services;

public sealed class BookLogic : IBookLogic
{
    private readonly ConcurrentDictionary<int, Book> _store = new();
    private int _nextId = 0;
    public Book Create(Book book)
    {
        if (book.Id <= 0) book.Id = Interlocked.Increment(ref _nextId);
        _store[book.Id] = Clone(book);
        return Clone(book);
    }

    public Book? Read(int id) =>
        _store.TryGetValue(id, out var b) ? Clone(b) : null;

    public IReadOnlyList<Book> ReadAll() =>
        _store.Values.Select(Clone).OrderBy(b => b.Id).ToList();

    public bool Update(Book book)
    {
        if (!_store.ContainsKey(book.Id)) return false;
        _store[book.Id] = Clone(book);
        return true;
    }

    public bool Delete(int id) => _store.TryRemove(id, out _);

    public IDictionary<string, List<Book>> GroupByGenre() =>
        _store.Values
              .GroupBy(b => string.IsNullOrWhiteSpace(b.Genre) ? "Не указан" : b.Genre)
              .OrderBy(g => g.Key)
              .ToDictionary(g => g.Key, g => g.Select(Clone).OrderBy(b => b.Id).ToList());

    public List<Book> FindByAuthor(string authorPart)
    {
        authorPart = authorPart?.Trim() ?? "";
        return _store.Values
            .Where(b => b.Author.Contains(authorPart, StringComparison.OrdinalIgnoreCase))
            .Select(Clone)
            .OrderBy(b => b.Id)
            .ToList();
    }

    private static Book Clone(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        Genre = b.Genre,
        Year = b.Year,
        Pages = b.Pages,
        IsRead = b.IsRead
    };
}
