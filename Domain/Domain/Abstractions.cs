using Domain.Models;

namespace Domain.Abstractions;

public interface IBookLogic
{
    // CRUD
    Book Create(Book book);
    Book? Read(int id);
    IReadOnlyList<Book> ReadAll();
    bool Update(Book book);
    bool Delete(int id);

    // Бизнес-функции
    IDictionary<string, List<Book>> GroupByGenre();
    List<Book> FindByAuthor(string authorPart);
}
