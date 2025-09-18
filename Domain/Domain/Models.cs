using System;

namespace Domain.Models
{
    /// <summary>Сущность «Книга».</summary>
    public sealed class Book
    {
        /// <summary>Уникальный идентификатор.</summary>
        public int Id { get; set; }
        /// <summary>Название.</summary>
        public string Title { get; set; } = "";
        /// <summary>Автор(ы).</summary>
        public string Author { get; set; } = "";
        /// <summary>Жанр (может быть пустым).</summary>
        public string Genre { get; set; } = "";
        /// <summary>Год издания (0..3000).</summary>
        public int Year { get; set; }
        /// <summary>Количество страниц (&gt;0).</summary>
        public int Pages { get; set; }
        /// <summary>Флаг «прочитана».</summary>
        public bool IsRead { get; set; }

        /// <summary>Читабельное представление.</summary>
        public override string ToString() => $"{Title} — {Author} ({Year}), {Genre}";
    }
}
