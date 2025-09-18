using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Domain.Abstractions;
using Domain.Models;

namespace Domain.Services
{
    /// <summary>Потокобезопасная in-memory реализация <see cref="IBookLogic"/>.</summary>
    public sealed class Logic : IBookLogic
    {
        private readonly ConcurrentDictionary<int, Book> _store = new();
        private int _nextId;

        /// <summary>Создать книгу (Id генерируется).</summary>
        /// <exception cref="ArgumentNullException"/><exception cref="ArgumentException"/><exception cref="ArgumentOutOfRangeException"/>
        public Book Create(Book book)
        {
            if (book is null) throw new ArgumentNullException(nameof(book));
            Validate(book, isNew: true);
            var copy = Clone(book); copy.Id = Interlocked.Increment(ref _nextId);
            _store[copy.Id] = Clone(copy);
            return Clone(copy);
        }

        /// <summary>Прочитать по Id.</summary>
        public Book? Read(int id) => _store.TryGetValue(id, out var b) ? Clone(b) : null;

        /// <summary>Все книги.</summary>
        public IReadOnlyList<Book> ReadAll() => _store.Values.Select(Clone).OrderBy(b => b.Id).ToList();

        /// <summary>Обновить существующую (по Id).</summary>
        /// <exception cref="ArgumentNullException"/><exception cref="ArgumentException"/><exception cref="ArgumentOutOfRangeException"/>
        public bool Update(Book book)
        {
            if (book is null) throw new ArgumentNullException(nameof(book));
            if (book.Id <= 0) return false;
            Validate(book, isNew: false);
            if (!_store.ContainsKey(book.Id)) return false;
            _store[book.Id] = Clone(book); return true;
        }

        /// <summary>Удалить по Id.</summary>
        public bool Delete(int id) => _store.TryRemove(id, out _);

        /// <summary>Группировка по жанрам.</summary>
        public IDictionary<string, List<Book>> GroupByGenre() =>
            _store.Values.GroupBy(b => string.IsNullOrWhiteSpace(b.Genre) ? "Не указан" : b.Genre)
                  .OrderBy(g => g.Key)
                  .ToDictionary(g => g.Key, g => g.Select(Clone).OrderBy(b => b.Id).ToList());

        /// <summary>Поиск по автору.</summary>
        public List<Book> FindByAuthor(string authorPart)
        {
            var needle = (authorPart ?? "").Trim();
            return _store.Values.Where(b => b.Author.Contains(needle, StringComparison.OrdinalIgnoreCase))
                                .Select(Clone).OrderBy(b => b.Id).ToList();
        }

        /// <summary>Проверка бизнес-ограничений.</summary>
        private static void Validate(Book b, bool isNew)
        {
            if (string.IsNullOrWhiteSpace(b.Title)) throw new ArgumentException("Название пусто.", nameof(b));
            if (string.IsNullOrWhiteSpace(b.Author) || b.Author.Any(char.IsDigit)) throw new ArgumentException("Автор обязателен и без цифр.", nameof(b));
            if (!string.IsNullOrWhiteSpace(b.Genre) && b.Genre.Any(char.IsDigit)) throw new ArgumentException("Жанр без цифр.", nameof(b));
            if (b.Year is < 0 or > 3000) throw new ArgumentOutOfRangeException(nameof(b.Year));
            if (b.Pages <= 0) throw new ArgumentOutOfRangeException(nameof(b.Pages));
            if (!isNew && b.Id <= 0) throw new ArgumentOutOfRangeException(nameof(b.Id));
        }

        /// <summary>Клонирование для изоляции хранилища.</summary>
        private static Book Clone(Book b) => new()
        { Id = b.Id, Title = b.Title, Author = b.Author, Genre = b.Genre, Year = b.Year, Pages = b.Pages, IsRead = b.IsRead };
    }
}
