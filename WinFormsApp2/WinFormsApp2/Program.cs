using System;
using System.Windows.Forms;
using Domain.Services;
using WinFormsApp.Forms;

namespace WinFormsApp2
{
    /// <summary>����� ����� WinForms.</summary>
    internal static class Program
    {
        /// <summary>STA-����.</summary>
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(new Logic()));
        }
    }
}
