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
            string asdasd = "asd".PadLeft(3);

            Console.WriteLine("Hello World!");
            var Db = new DojoManagerApi.TestNHibernate();

            Db.Test1();
            Db.Test2();
            Db.Test3();
            Db.Test_Deletions();

            Db.DeleteDb();
            Db.Initialize();
            Db.Populate();

            PritSubscriptions(Db);

        }

        public bool Test(object asd)
        {
            var t = asd as Address;
            if (t != null)
                return this.Equals(t.Number);
            else
                return this.Equals(asd);
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
