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

namespace DojoManagerGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DojoManagerApi.DbManager Db;

        public static string ClubName { get; set; } = "Ken Sei Dojo";

        protected override void OnStartup(StartupEventArgs e)
        {
            Db = new DojoManagerApi.DbManager(Config.Instance.DbName, Config.Instance.DbLocation);
            Db.ClearDatabase();
            Db.Load();
            TestNHibernate.PopulateDb(Db);
            Db.Save();

            var persons = Db.ListPersons();
            var proxy = (Person)EntityWrapper.Wrap(persons.First());
            (proxy as INotifyPropertyChanged).PropertyChanged +=
                (s, e) =>
                    Console.WriteLine($"{e} has changed.");

            Window_Main window = new Window_Main();
            window.Show();

        }
    }
}
