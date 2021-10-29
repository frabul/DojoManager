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
        protected override void OnStartup(StartupEventArgs e)
        {
            Db = new DojoManagerApi.DbManager(Config.Instance.DbName, Config.Instance.DbLocation);
            //Db.ClearDatabase();
            Db.Load();
            //TestNHibernate.PopulateDb(Db);
            //Db.Save();


            SaveTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
            SaveTimer.Tick += (s, e) =>
            {
                if(App.Db.IsOpen)
                App.Db?.Save();
            };
            SaveTimer.Start();


            Window_Main window = new Window_Main();
            window.DataContext = new VM_MainWindow();
            window.Show();

        }
    }
}
