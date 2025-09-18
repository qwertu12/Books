using System;
using System.Linq;
using Domain.Abstractions;
using Domain.Models;
using Domain.Services;

namespace ConsoleApp
{
    /// <summary>Консоль для работы с книгами (CRUD + группы/поиск).</summary>
    internal static class Program
    {
        /// <summary>Входная точка.</summary>
        private static void Main()
        {
            IBookLogic logic = new Logic();
            // демо-данные
            logic.Create(new Book { Title = "CLR via C#", Author = "Jeffrey Richter", Genre = "Programming", Year = 2012, Pages = 900 });
            logic.Create(new Book { Title = "Clean Code", Author = "Robert C. Martin", Genre = "Programming", Year = 2008, Pages = 464, IsRead = true });
            logic.Create(new Book { Title = "Dune", Author = "Frank Herbert", Genre = "Sci-Fi", Year = 1965, Pages = 592 });

            while (true)
            {
                Console.WriteLine("1) Список  2) Добавить  3) Изменить  4) Удалить  5) Группы  6) Поиск  0) Выход");
                switch (ReadInt("Выбор: ", 0, 6))
                {
                    case 1:
                        var all = logic.ReadAll();
                        Console.WriteLine(all.Count == 0 ? "Список пуст." : string.Join(Environment.NewLine, all.Select(b => $"{b.Id} :: {b}")));
                        break;
                    case 2:
                        Try(() => { var created = logic.Create(InputBook()); Console.WriteLine($"Добавлено. Id={created.Id}"); });
                        break;
                    case 3:
                        Try(() =>
                        {
                            var id = ReadExistingId(logic, "Id для изменения: ");
                            var edited = EditBook(logic.Read(id)!);
                            Console.WriteLine(logic.Update(edited) ? "Сохранено." : "Не найдено.");
                        });
                        break;
                    case 4:
                        var delId = ReadExistingId(logic, "Id для удаления: ");
                        Console.WriteLine(logic.Delete(delId) ? "Удалено." : "Не найдено.");
                        break;
                    case 5:
                        var groups = logic.GroupByGenre();
                        Console.WriteLine(groups.Count == 0 ? "Пусто." :
                            string.Join(Environment.NewLine, groups.Select(g => $"[{g.Key}] {g.Value.Count} шт.")));
                        break;
                    case 6:
                        var q = ReadString("Автор (часть, можно пусто): ", true);
                        var res = logic.FindByAuthor(q);
                        Console.WriteLine(res.Count == 0 ? "Ничего не найдено." : string.Join(Environment.NewLine, res));
                        break;
                    case 0: return;
                }
                Console.WriteLine();
            }
        }

        /// <summary>Безопасный вызов действия с выводом сообщения об ошибке.</summary>
        private static void Try(Action action)
        {
            try { action(); } catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
        }

        /// <summary>Чтение Int с диапазоном.</summary>
        private static int ReadInt(string prompt, int? min = null, int? max = null)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var v) &&
                    (!min.HasValue || v >= min) && (!max.HasValue || v <= max)) return v;
                Console.WriteLine($"Введите целое число{(min.HasValue || max.HasValue ? $" [{min ?? int.MinValue}; {max ?? int.MaxValue}]" : "")}.");
            }
        }
        /// <summary>Чтение строки (непустой, если <paramref name="allowEmpty"/> = false).</summary>
        private static string ReadString(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (allowEmpty || !string.IsNullOrWhiteSpace(s)) return s;
                Console.WriteLine("Поле не может быть пустым.");
            }
        }
        /// <summary>Строка без цифр (для автора/жанра).</summary>
        private static string ReadTextNoDigits(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                var s = ReadString(prompt, allowEmpty);
                if (allowEmpty && s == "") return s;
                if (!s.Any(char.IsDigit)) return s;
                Console.WriteLine("Текст не должен содержать цифр.");
            }
        }
        /// <summary>Да/нет.</summary>
        private static bool ReadYesNo(string prompt, bool? def = null)
        {
            while (true)
            {
                Console.Write($"{prompt}{(def is null ? " (y/n): " : def.Value ? " (Y/n): " : " (y/N): ")}");
                var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(s) && def is not null) return def.Value;
                if (s is "y" or "д") return true; if (s is "n" or "н") return false;
            }
        }
        /// <summary>Существующий Id.</summary>
        private static int ReadExistingId(IBookLogic logic, string prompt)
        {
            while (true) { var id = ReadInt(prompt, 1, null); if (logic.Read(id) is not null) return id; Console.WriteLine("Не найдено."); }
        }
        /// <summary>Ввод новой книги.</summary>
        private static Book InputBook()
        {
            return new Book
            {
                Title = ReadString("Название: "),
                Author = ReadTextNoDigits("Автор: "),
                Genre = ReadTextNoDigits("Жанр: ", true),
                Year = ReadInt("Год (0..3000): ", 0, 3000),
                Pages = ReadInt("Страниц (>0): ", 1, null),
                IsRead = ReadYesNo("Прочитана?", false)
            };
        }
        /// <summary>Редактирование книги.</summary>
        private static Book EditBook(Book b)
        {
            var t = ReadString($"Название ({b.Title}): ", true);
            var a = ReadTextNoDigits($"Автор ({b.Author}): ", true);
            var g = ReadTextNoDigits($"Жанр ({b.Genre}): ", true);
            b.Title = string.IsNullOrWhiteSpace(t) ? b.Title : t;
            b.Author = string.IsNullOrWhiteSpace(a) ? b.Author : a;
            b.Genre = string.IsNullOrWhiteSpace(g) ? b.Genre : g;
            b.Year = ReadInt("Год: ", 0, 3000);
            b.Pages = ReadInt("Страниц: ", 1, null);
            b.IsRead = ReadYesNo($"Прочитана сейчас {(b.IsRead ? "да" : "нет")}?", b.IsRead);
            return b;
        }
    }
}
