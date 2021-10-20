using DojoManagerApi;
using DojoManagerApi.Entities;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Db.DeleteDb();
            Db.Initialize();
            Db.Populate();

            PritSubscriptions(Db);

            var persons = Db.ListPersons();
            foreach (var person in persons)
            {
                person.Subscriptions = new ObservableCollection<Subscription>(person.Subscriptions);
                person.Subscriptions.RemoveAt(0);
                person.AddSubscription(new Subscription() { Notes = "nuovo" }, 123);
                person.AddCard(new Card() { CardId = "ma che cazzo", ExpirationDate = DateTime.Now, Type = CardType.Kensei });
            }

            Db.Flush();
            Db.Close();


            Console.WriteLine("\n--- Seconda parte ----");
            Db.Initialize();
            PritSubscriptions(Db);

            //var persona = Db.ListPersons().First();
            //persona.Name = "ziopippo";
            Db.Close();
            Db.Initialize();
            var p = Db.ListPersons().First();
            var proxy = (Person)EntityWrapper.Wrap(Db.ListPersons().First());
            proxy.Name = "marcantonio";
            proxy.AddCard(new Card() { CardId = "hey you", ExpirationDate = DateTime.Now, Type = CardType.Kensei });
            proxy.Subscriptions.Remove(proxy.Subscriptions.Last());
            var sub = (Subscription)EntityWrapper.Wrap(proxy.Subscriptions[0]);
            var deb = sub.Debit;
            Db.Flush();
            proxy = (Person)EntityWrapper.Wrap(Db.ListPersons().First());
            sub = (Subscription)EntityWrapper.Wrap(proxy.Subscriptions[0]);
            Console.WriteLine("sub cost " + sub.Cost);
            Db.Flush();

            Console.WriteLine("\n\nTerzo giro");
            Db.Close();
            Db.Initialize();
            PritSubscriptions(Db);

            (proxy as INotifyPropertyChanged).PropertyChanged +=
                (s, e) =>
                    Console.WriteLine($"{e} has changed.");


        }

        private static void PritSubscriptions(TestNHibernate Db)
        {
            foreach (var p in Db.ListPersons())
            {
                Console.WriteLine(p.Name);
                foreach (var sub in p.Subscriptions)
                {
                    Console.WriteLine($"   Sub {sub.Notes} - {sub.Cost}");
                }
            }
        }
    }
}
