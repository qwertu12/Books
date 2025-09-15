using System;
using System.Windows.Forms;
using Domain.Models;

namespace WinFormsApp.Forms;

public sealed class BookEditForm : Form
{

    private static void BlockDigits(object? sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar)) return;   // Backspace, Delete, Enter и т.п.
        if (char.IsDigit(e.KeyChar))             // цифры запрещаем
            e.Handled = true;
    }


    private readonly TextBox tbTitle = new() { Width = 300 };
    private readonly TextBox tbAuthor = new() { Width = 300 };
    private readonly TextBox tbGenre = new() { Width = 200 };
    private readonly NumericUpDown numYear = new() { Minimum = 0, Maximum = 3000, Value = 2000 };
    private readonly NumericUpDown numPages = new() { Minimum = 1, Maximum = 10000, Value = 100 };
    private readonly CheckBox cbRead = new() { Text = "Прочитана" };
    private readonly Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK };
    private readonly Button btnCancel = new() { Text = "Отмена", DialogResult = DialogResult.Cancel };

    public Book Value { get; private set; }

    public BookEditForm(Book book, bool isEdit = false)
    {

        Value = new Book
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Genre = book.Genre,
            Year = book.Year,
            Pages = book.Pages,
            IsRead = book.IsRead
        };

        Text = isEdit ? "Изменить книгу" : "Добавить книгу";
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        var grid = new TableLayoutPanel { ColumnCount = 2, RowCount = 6, Padding = new Padding(10), AutoSize = true };
        grid.Controls.Add(new Label { Text = "Название:" }, 0, 0); grid.Controls.Add(tbTitle, 1, 0);
        grid.Controls.Add(new Label { Text = "Автор:" }, 0, 1); grid.Controls.Add(tbAuthor, 1, 1);
        grid.Controls.Add(new Label { Text = "Жанр:" }, 0, 2); grid.Controls.Add(tbGenre, 1, 2);
        grid.Controls.Add(new Label { Text = "Год:" }, 0, 3); grid.Controls.Add(numYear, 1, 3);
        grid.Controls.Add(new Label { Text = "Страниц:" }, 0, 4); grid.Controls.Add(numPages, 1, 4);
        grid.Controls.Add(cbRead, 1, 5);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, AutoSize = true };
        btnOk.Text = isEdit ? "Сохранить" : "Добавить";   // <<< нужная надпись
        btnOk.DialogResult = DialogResult.OK;
        btnCancel.Text = "Отмена";
        btnCancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.AddRange(new Control[] { btnOk, btnCancel });

        Controls.Add(grid);
        Controls.Add(buttons);

        // init values
        tbTitle.Text = Value.Title;
        tbAuthor.Text = Value.Author;
        tbGenre.Text = Value.Genre;
        numYear.Value = Math.Clamp(Value.Year, (int)numYear.Minimum, (int)numYear.Maximum);
        numPages.Value = Math.Clamp(Value.Pages, (int)numPages.Minimum, (int)numPages.Maximum);
        cbRead.Checked = Value.IsRead;

        // чтобы Enter нажимал основную кнопку
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        // простая валидация + перенос данных обратно в Value
        btnOk.Click += (_, __) =>
        {
            if (string.IsNullOrWhiteSpace(tbTitle.Text))
            {
                MessageBox.Show("Введите название книги.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None; // не закрывать окно
                return;
            }
            if (tbAuthor.Text.Any(char.IsDigit))
            {
                MessageBox.Show("Поле «Автор» не должно содержать цифр.", "Проверка");
                DialogResult = DialogResult.None; tbAuthor.Focus(); return;
            }
            if (tbGenre.Text.Any(char.IsDigit))
            {
                MessageBox.Show("Поле «Жанр» не должно содержать цифр.", "Проверка");
                DialogResult = DialogResult.None; tbGenre.Focus(); return;
            }

            Value.Title = tbTitle.Text.Trim();
            Value.Author = tbAuthor.Text.Trim();
            Value.Genre = tbGenre.Text.Trim();
            tbAuthor.KeyPress += BlockDigits; // запрет цифр в "Автор"
            tbGenre.KeyPress += BlockDigits; // запрет цифр в "Жанр"
            Value.Year = (int)numYear.Value;
            Value.Pages = (int)numPages.Value;
            Value.IsRead = cbRead.Checked;

        };
    }
}