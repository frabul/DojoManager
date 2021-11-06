using DojoManagerApi;
using DojoManagerApi.Entities;
using DojoManagerGui.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

using System.Diagnostics;
using Gehtsoft.PDFFlow.Builder;
using Gehtsoft.PDFFlow.Models.Shared;

namespace DojoManagerGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DojoManagerApi.DbManager Db;

        public static string ClubName { get; set; } = "Ken Sei Dojo";
        private DispatcherTimer SaveTimer;
        private static Window_Main MainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            //PdfTest.Test();
   

            Db = new DojoManagerApi.DbManager(Config.Instance.DbName, Config.Instance.DbLocation);
            //Db.ClearDatabase();
            Db.Load();
            //TestNHibernate.PopulateDb(Db);
            //Db.Save();


            SaveTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
            SaveTimer.Tick += (s, e) =>
            {
                if (App.Db.IsOpen)
                    App.Db?.Save();
            };
            SaveTimer.Start();


            MainWindow = new Window_Main();

            MainWindow.DataContext = new VM_MainWindow();
            MainWindow.Show();

        }

        public static Task<MessageDialogResult> ShowMessage(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            return MainWindow.ShowMessageAsync(title, message, style);
        }
        public static async Task AskAndExecuteAsync(Action act)
        {
            var res = await App.ShowMessage(
                "Attenzione", 
                "Sei sicuro di voler rimuovere l'oggetto?",
                MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative);
            if (res == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                act.Invoke();
            }
        }
        public static string? SelectImage()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.bmp) | *.png;*.jpeg;*.bmp";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFile = openFileDialog.FileName;
                return selectedFile;
            }
            return null;
        }
    }
}
