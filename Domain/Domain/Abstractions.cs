using System.Collections.Generic;
using Domain.Models;

namespace Domain.Abstractions
{
    /// <summary>Контракт бизнес-логики для книг.</summary>
    public interface IBookLogic
    {
        /// <summary>Создать книгу.</summary>
        /// <returns>Созданная копия с Id.</returns>
        Book Create(Book book);
        /// <summary>Прочитать по Id.</summary>
        Book? Read(int id);
        /// <summary>Прочитать все книги.</summary>
        IReadOnlyList<Book> ReadAll();
        /// <summary>Обновить существующую книгу.</summary>
        bool Update(Book book);
        /// <summary>Удалить по Id.</summary>
        bool Delete(int id);
        /// <summary>Группировка по жанрам (пустой → «Не указан»).</summary>
        IDictionary<string, List<Book>> GroupByGenre();
        /// <summary>Поиск по автору (подстрока, регистр не важен).</summary>
        List<Book> FindByAuthor(string authorPart);
    }
}
