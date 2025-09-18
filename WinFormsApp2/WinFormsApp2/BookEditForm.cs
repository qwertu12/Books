using System;
using System.Linq;
using System.Windows.Forms;
using Domain.Models;

namespace WinFormsApp.Forms
{
    /// <summary>Диалог добавления/редактирования книги.</summary>
    public sealed class BookEditForm : Form
    {
        private static void BlockDigits(object? s, KeyPressEventArgs e)
        { if (!char.IsControl(e.KeyChar) && char.IsDigit(e.KeyChar)) e.Handled = true; }

        private readonly TextBox tbTitle = new() { Width = 280 };
        private readonly TextBox tbAuthor = new() { Width = 280 };
        private readonly TextBox tbGenre = new() { Width = 180 };
        private readonly NumericUpDown numYear = new() { Minimum = 0, Maximum = 3000, Value = 2000 };
        private readonly NumericUpDown numPages = new() { Minimum = 1, Maximum = 10000, Value = 100 };
        private readonly CheckBox cbRead = new() { Text = "Прочитана" };
        private readonly Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK };
        private readonly Button btnCancel = new() { Text = "Отмена", DialogResult = DialogResult.Cancel };

        /// <summary>Значение книги, заполненное по данным формы.</summary>
        public Book Value { get; private set; }

        /// <summary>Создаёт форму.</summary>
        public BookEditForm(Book book, bool isEdit = false)
        {
            Value = new Book
            { Id = book.Id, Title = book.Title, Author = book.Author, Genre = book.Genre, Year = book.Year, Pages = book.Pages, IsRead = book.IsRead };

            Text = isEdit ? "Изменить книгу" : "Добавить книгу";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var grid = new TableLayoutPanel { ColumnCount = 2, RowCount = 6, Padding = new Padding(10), AutoSize = true };
            grid.Controls.Add(new Label { Text = "Название:" }, 0, 0); grid.Controls.Add(tbTitle, 1, 0);
            grid.Controls.Add(new Label { Text = "Автор:" }, 0, 1); grid.Controls.Add(tbAuthor, 1, 1);
            grid.Controls.Add(new Label { Text = "Жанр:" }, 0, 2); grid.Controls.Add(tbGenre, 1, 2);
            grid.Controls.Add(new Label { Text = "Год:" }, 0, 3); grid.Controls.Add(numYear, 1, 3);
            grid.Controls.Add(new Label { Text = "Страниц:" }, 0, 4); grid.Controls.Add(numPages, 1, 4);
            grid.Controls.Add(cbRead, 1, 5);

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, AutoSize = true };
            btnOk.Text = isEdit ? "Сохранить" : "Добавить";
            buttons.Controls.AddRange(new Control[] { btnOk, btnCancel });

            Controls.Add(grid); Controls.Add(buttons);

            tbTitle.Text = Value.Title; tbAuthor.Text = Value.Author; tbGenre.Text = Value.Genre;
            numYear.Value = Math.Clamp(Value.Year, (int)numYear.Minimum, (int)numYear.Maximum);
            numPages.Value = Math.Clamp(Value.Pages, (int)numPages.Minimum, (int)numPages.Maximum);
            cbRead.Checked = Value.IsRead;

            tbAuthor.KeyPress += BlockDigits; tbGenre.KeyPress += BlockDigits;
            AcceptButton = btnOk; CancelButton = btnCancel;

            btnOk.Click += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(tbTitle.Text))
                { MessageBox.Show("Введите название.", "Проверка"); DialogResult = DialogResult.None; return; }
                if (string.IsNullOrWhiteSpace(tbAuthor.Text) || tbAuthor.Text.Any(char.IsDigit))
                { MessageBox.Show("Автор обязателен и без цифр.", "Проверка"); DialogResult = DialogResult.None; return; }
                if (tbGenre.Text.Any(char.IsDigit))
                { MessageBox.Show("Жанр без цифр.", "Проверка"); DialogResult = DialogResult.None; return; }

                Value.Title = tbTitle.Text.Trim();
                Value.Author = tbAuthor.Text.Trim();
                Value.Genre = tbGenre.Text.Trim();
                Value.Year = (int)numYear.Value;
                Value.Pages = (int)numPages.Value;
                Value.IsRead = cbRead.Checked;
            };
        }
    }
}
