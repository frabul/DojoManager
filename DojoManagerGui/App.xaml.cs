using DojoManagerApi;
using DojoManagerApi.Entities;
using DojoManagerGui.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
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
        public static DojoManagerApi.TestNHibernate Db;
        protected override void OnStartup(StartupEventArgs e)
        {
            Db = new DojoManagerApi.TestNHibernate();
            Db.Initialize();
            //var decorated = EntitiesViewModelProxy<IDebit>.Create(Db.ListPersons().SelectMany(p => p.Debits).FirstOrDefault() );
            var persons = Db.ListPersons();
            var proxy = (Person)EntityWrapper.Wrap(persons.First() );
            (proxy as INotifyPropertyChanged).PropertyChanged +=
                (s, e) => 
                    Console.WriteLine($"{e} has changed.");
            proxy.Name = "asdasdasdasd";
            MainWindow window = new MainWindow(); 
            window.Show();

        }
    }
}
