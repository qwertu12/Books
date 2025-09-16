using System;
using System.Linq;
using Domain.Abstractions;
using Domain.Models;
using Domain.Services;

IBookLogic logic = new BookLogic();

// демоданные
logic.Create(new Book { Title = "CLR via C#", Author = "Jeffrey Richter", Genre = "Programming", Year = 2012, Pages = 900 });
logic.Create(new Book { Title = "Clean Code", Author = "Robert C. Martin", Genre = "Programming", Year = 2008, Pages = 464, IsRead = true });
logic.Create(new Book { Title = "Dune", Author = "Frank Herbert", Genre = "Sci-Fi", Year = 1965, Pages = 592 });

while (true)
{
    Console.WriteLine("""
    1) Список книг
    2) Добавить
    3) Изменить
    4) Удалить
    5) Группировка по жанрам
    6) Поиск по автору
    0) Выход
    """);

    var key = ReadMenuChoice(0, 6);

    switch (key)
    {
        case 1:
            if (logic.ReadAll().Count == 0) { Console.WriteLine("Список пуст."); break; }
            foreach (var b in logic.ReadAll()) Console.WriteLine($"{b.Id} :: {b}");
            break;

        case 2:
            var nb = InputBook();
            logic.Create(nb);
            Console.WriteLine("Добавлено.");
            break;

        case 3:
            {
                var id = ReadExistingId(logic, "Id книги для изменения: ");
                var book = logic.Read(id)!;
                var edited = EditBook(book);
                logic.Update(edited);
                Console.WriteLine("Сохранено.");
                break;
            }

        case 4:
            {
                var id = ReadExistingId(logic, "Id книги для удаления: ");
                logic.Delete(id);
                Console.WriteLine("Удалено.");
                break;
            }

        case 5:
            {
                var groups = logic.GroupByGenre();
                if (groups.Count == 0) { Console.WriteLine("Пусто."); break; }
                foreach (var kv in groups)
                {
                    Console.WriteLine($"[{kv.Key}]");
                    foreach (var b in kv.Value) Console.WriteLine("  - " + b);
                }
                break;
            }

        case 6:
            {
                var q = ReadString("Автор (часть, можно пусто): ", allowEmpty: true);
                var res = logic.FindByAuthor(q);
                Console.WriteLine(res.Count == 0 ? "Ничего не найдено." : string.Join(Environment.NewLine, res.Select(b => b.ToString())));
                break;
            }

        case 0:
            return;
    }

    Console.WriteLine();
}

// -------------------- ВАЛИДАЦИЯ/ХЕЛПЕРЫ --------------------

static int ReadMenuChoice(int min, int max)
{
    while (true)
    {
        Console.Write("Выбор: ");
        var s = Console.ReadLine();
        if (int.TryParse(s, out int k) && k >= min && k <= max) return k;
        Console.WriteLine($"Введите число от {min} до {max}.");
    }
}

static int ReadInt(string prompt, int? min = null, int? max = null)
{
    while (true)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (int.TryParse(s, out int v) &&
            (!min.HasValue || v >= min.Value) &&
            (!max.HasValue || v <= max.Value))
            return v;
        Console.WriteLine($"Введите целое число{(min.HasValue || max.HasValue ? $" в диапазоне [{min ?? int.MinValue}; {max ?? int.MaxValue}]" : "")}.");
    }
}

static int ReadOptionalInt(string prompt, int current, int? min = null, int? max = null)
{
    while (true)
    {
        Console.Write($"{prompt} ({current}): ");
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return current; // оставить как есть
        if (int.TryParse(s, out int v) &&
            (!min.HasValue || v >= min.Value) &&
            (!max.HasValue || v <= max.Value))
            return v;
        Console.WriteLine($"Введите целое число{(min.HasValue || max.HasValue ? $" в диапазоне [{min ?? int.MinValue}; {max ?? int.MaxValue}]" : "")} или оставьте пусто.");
    }
}

static string ReadString(string prompt, bool allowEmpty = false)
{
    while (true)
    {
        Console.Write(prompt);
        var s = Console.ReadLine() ?? "";
        if (allowEmpty || !string.IsNullOrWhiteSpace(s)) return s.Trim();
        Console.WriteLine("Поле не может быть пустым.");
    }
}

static string ReadTextNoDigits(string prompt, bool allowEmpty = false)
{
    while (true)
    {
        var s = ReadString(prompt, allowEmpty);
        if (allowEmpty && string.IsNullOrWhiteSpace(s)) return "";
        // защита: не число целиком и без цифр внутри
        if (!int.TryParse(s, out _) && !s.Any(char.IsDigit)) return s;
        Console.WriteLine("Текст не должен содержать цифр и не должен быть чисто числом.");
    }
}

static bool ReadYesNo(string prompt, bool? defaultValue = null)
{
    while (true)
    {
        Console.Write($"{prompt}{(defaultValue is null ? " (y/n): " : defaultValue.Value ? " (Y/n): " : " (y/N): ")}");
        var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(s) && defaultValue is not null) return defaultValue.Value;
        if (s == "y" || s == "д") return true;
        if (s == "n" || s == "н") return false;

        Console.WriteLine("Введите y (да) или n (нет).");
    }
}

static int ReadExistingId(IBookLogic logic, string prompt)
{
    while (true)
    {
        var id = ReadInt(prompt, min: 1);
        if (logic.Read(id) is not null) return id;
        Console.WriteLine("Книга с таким Id не найдена. Повторите ввод.");
    }
}

// -------------------- CRUD-ВВОД --------------------

static Book InputBook()
{
    var title = ReadString("Название: ");
    var author = ReadTextNoDigits("Автор: ");
    var genre = ReadTextNoDigits("Жанр: ", allowEmpty: true);
    var year = ReadInt("Год (0..3000): ", 0, 3000);
    var pages = ReadInt("Страниц (>0): ", 1, null);
    var isRead = ReadYesNo("Прочитана?", defaultValue: false);

    return new Book { Title = title, Author = author, Genre = genre, Year = year, Pages = pages, IsRead = isRead };
}

static Book EditBook(Book b)
{
    var title = ReadString($"Название ({b.Title}): ", allowEmpty: true);
    var author = ReadTextNoDigits($"Автор ({b.Author}): ", allowEmpty: true);
    var genre = ReadTextNoDigits($"Жанр ({b.Genre}): ", allowEmpty: true);
    var year = ReadOptionalInt("Год", b.Year, 0, 3000);
    var pages = ReadOptionalInt("Страниц", b.Pages, 1, null);
    var isRead = ReadYesNo($"Прочитана сейчас {(b.IsRead ? "да" : "нет")}?", defaultValue: b.IsRead);

    if (!string.IsNullOrWhiteSpace(title)) b.Title = title;
    if (!string.IsNullOrWhiteSpace(author)) b.Author = author;
    if (!string.IsNullOrWhiteSpace(genre)) b.Genre = genre;
    b.Year = year;
    b.Pages = pages;
    b.IsRead = isRead;

    return b;
}
