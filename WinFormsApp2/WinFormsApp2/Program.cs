using Domain.Services;
using System;
using System.Windows.Forms;
using WinFormsApp.Forms;

namespace WinFormsApp2;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var logic = new BookLogic();
        Application.Run(new MainForm(logic));

    }
}
