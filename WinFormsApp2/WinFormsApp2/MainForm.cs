using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Domain.Abstractions;
using Domain.Models;
using System.Drawing;

namespace WinFormsApp.Forms;

public sealed class MainForm : Form
{
    private readonly IBookLogic _logic;
    private readonly BindingList<Book> _data = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly TextBox _filterAuthor = new() { PlaceholderText = "Фильтр по автору..." };
    private readonly Button _btnAdd = new() { Text = "Добавить" };
    private readonly Button _btnEdit = new() { Text = "Изменить" };
    private readonly Button _btnDel = new() { Text = "Удалить" };
    private readonly Button _btnGroups = new() { Text = "Группировка по жанрам" };



    // добавь поле панели в класс:
    private readonly FlowLayoutPanel _top = new();

    // в конструкторе MainForm:
    public MainForm(IBookLogic logic)
    {
        _logic = logic;

        Text = "Библиотека (WinForms)";
        Width = 1000; Height = 650;

        _top.Controls.AddRange(new Control[] { _filterAuthor, _btnAdd, _btnEdit, _btnDel, _btnGroups });
        _grid.DataSource = _data;

        Controls.Add(_grid);
        Controls.Add(_top);

        StyleToolbar(); // <<< ключевая строка

        Load += (_, __) => Reload();
        _filterAuthor.TextChanged += (_, __) => ApplyFilter();
        _btnAdd.Click += (_, __) => AddBook();
        _btnEdit.Click += (_, __) => EditSelected();
        _btnDel.Click += (_, __) => DeleteSelected();
        _btnGroups.Click += (_, __) => ShowGroups();


        // === добавь внутрь класса MainForm ===
        void StyleToolbar()
        {
            // сама панель сверху
            _top.Dock = DockStyle.Top;
            _top.AutoSize = false;     // фиксированная высота панели
            _top.Height = 48;          // высота «полоски» с кнопками
            _top.WrapContents = false; // всё в одну строку
            _top.Padding = new Padding(8);
            _top.Margin = new Padding(0);

            // поле фильтра
            _filterAuthor.AutoSize = false;
            _filterAuthor.Size = new Size(260, 32);
            _filterAuthor.Margin = new Padding(0, 8, 8, 8);

            // кнопки — одинаковый размер
            foreach (var b in new[] { _btnAdd, _btnEdit, _btnDel, _btnGroups })
            {
                b.AutoSize = false;
                b.Size = new Size(120, 32); // ширина/высота кнопки
                b.Margin = new Padding(6, 8, 0, 8);
                b.Font = new Font("Segoe UI", 10f);
            }

            // таблица
            _grid.Dock = DockStyle.Fill;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.RowHeadersVisible = false;
            _grid.ColumnHeadersHeight = 32;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

    }




    private void Reload()
    {
        _data.Clear();
        foreach (var b in _logic.ReadAll()) _data.Add(b);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _filterAuthor.Text?.Trim() ?? "";
        var items = string.IsNullOrEmpty(q) ? _logic.ReadAll() : _logic.FindByAuthor(q);
        _data.Clear();
        foreach (var b in items) _data.Add(b);
    }

    private void AddBook()
    {
        using var dlg = new BookEditForm(new Book(), isEdit: false); // надпись "Добавить"
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _logic.Create(dlg.Value);
            Reload();
        }
    }

    private void EditSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Book b) return;

        using var dlg = new BookEditForm(b, isEdit: true); // надпись "Сохранить"
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _logic.Update(dlg.Value);
            Reload();
        }
    }


    private void DeleteSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Book b) return;
        if (MessageBox.Show($"Удалить «{b.Title}»?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            _logic.Delete(b.Id);
            Reload();
        }
    }

    private void ShowGroups()
    {
        var groups = _logic.GroupByGenre();
        var text = string.Join(Environment.NewLine,
            groups.Select(g => $"[{g.Key}] {g.Value.Count} шт."));
        MessageBox.Show(string.IsNullOrEmpty(text) ? "Пусто" : text, "Жанры");
    }
}
