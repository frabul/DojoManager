using DojoManagerApi;
using DojoManagerApi.Entities;
using System;
using System.ComponentModel;
using System.Linq;

namespace DojoManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var Db = new DojoManagerApi.TestNHibernate();
            Db.Test();

            var persons = Db.ListPersons();
            var proxy = (Person)EntityWrapper.Wrap(persons.First(), typeof(Person));
            (proxy as INotifyPropertyChanged).PropertyChanged +=
                (s, e) =>
                    Console.WriteLine($"{e} has changed.");
            proxy.Name = "asdasdasdasd";
       
        }
    }
}
