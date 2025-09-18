using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Domain.Abstractions;
using Domain.Models;

namespace WinFormsApp.Forms
{
    /// <summary>Главная форма WinForms.</summary>
    public sealed class MainForm : Form
    {
        private readonly IBookLogic _logic;
        private readonly BindingList<Book> _data = new();

        private readonly TextBox _filter = new() { PlaceholderText = "Фильтр по автору..." };
        private readonly Button _add = new() { Text = "Добавить" };
        private readonly Button _edit = new() { Text = "Изменить" };
        private readonly Button _del = new() { Text = "Удалить" };
        private readonly Button _grp = new() { Text = "Группировка" };
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };

        /// <summary>Конструктор.</summary>
        public MainForm(IBookLogic logic)
        {
            _logic = logic ?? throw new ArgumentNullException(nameof(logic));
            Text = "Библиотека (WinForms)"; Width = 1200; Height = 600; StartPosition = FormStartPosition.CenterScreen;

            // Верхняя панель
            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8) };
            top.Controls.AddRange(new Control[] { _filter, _add, _edit, _del, _grp });
            void Style(Button b)
            {
                b.AutoSize = false;
                b.Size = new Size(150, 40);                 // ← размеры кнопок
                b.Margin = new Padding(6, 8, 0, 8);
                b.Font = new Font("Segoe UI", 10.5f);       // ↑ шрифт = выше кнопка
            }
            Style(_add); Style(_edit); Style(_del); Style(_grp);

            _filter.Width = 320;                            // поле поиска пошире/повыше
                                                            // чтобы высота считалась по шрифту формы
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Font = new Font("Segoe UI", 10.5f);


            // Макет: панель сверху + грид
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.RowHeadersVisible = false; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect = false;
            _grid.DataSource = _data;
            root.Controls.Add(top, 0, 0); root.Controls.Add(_grid, 0, 1);
            Controls.Add(root);

            Load += (_, __) => Reload();
            _filter.TextChanged += (_, __) => ApplyFilter();
            _add.Click += (_, __) => AddBook();
            _edit.Click += (_, __) => EditSelected();
            _del.Click += (_, __) => DeleteSelected();
            _grp.Click += (_, __) => ShowGroups();
        }

        /// <summary>Перезагрузка таблицы.</summary>
        private void Reload()
        {
            _data.RaiseListChangedEvents = false; _data.Clear();
            foreach (var b in _logic.ReadAll()) _data.Add(b);
            _data.RaiseListChangedEvents = true; _data.ResetBindings();
            ApplyFilter();
        }

        /// <summary>Фильтр по автору.</summary>
        private void ApplyFilter()
        {
            var q = _filter.Text?.Trim() ?? "";
            var items = string.IsNullOrEmpty(q) ? _logic.ReadAll() : _logic.FindByAuthor(q);
            _data.RaiseListChangedEvents = false; _data.Clear();
            foreach (var b in items) _data.Add(b);
            _data.RaiseListChangedEvents = true; _data.ResetBindings();
        }

        /// <summary>Добавить.</summary>
        private void AddBook()
        {
            using var dlg = new BookEditForm(new Book());
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try { var created = _logic.Create(dlg.Value); Reload(); MessageBox.Show($"Добавлено. Id={created.Id}"); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка добавления"); }
            }
        }

        /// <summary>Редактировать.</summary>
        private void EditSelected()
        {
            if (_grid.CurrentRow?.DataBoundItem is not Book b) return;
            using var dlg = new BookEditForm(b, true);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try { if (_logic.Update(dlg.Value)) Reload(); else MessageBox.Show("Запись не найдена."); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка сохранения"); }
            }
        }

        /// <summary>Удалить.</summary>
        private void DeleteSelected()
        {
            if (_grid.CurrentRow?.DataBoundItem is not Book b) return;
            if (MessageBox.Show($"Удалить «{b.Title}»?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                if (_logic.Delete(b.Id)) Reload(); else MessageBox.Show("Запись не найдена.");
        }

        /// <summary>Показать группировку по жанрам.</summary>
        private void ShowGroups()
        {
            var g = _logic.GroupByGenre();
            var text = g.Count == 0 ? "Пусто" : string.Join(Environment.NewLine, g.Select(kv => $"[{kv.Key}] {kv.Value.Count} шт."));
            MessageBox.Show(text, "Жанры");
        }
    }
}
