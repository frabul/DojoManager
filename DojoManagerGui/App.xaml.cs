using DojoManagerApi.Entities;
using DojoManagerGui.ViewModels;
using System;
using System.Collections.Generic;
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
             
            var decorated = EntitiesViewModelProxy<IPerson>.Create(Db.ListPersons().First());
           
            MainWindow window = new MainWindow(); 
            window.Show();

        }
    }
}
