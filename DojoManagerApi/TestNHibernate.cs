using DojoManagerApi.Entities;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi
{
    public class AutomapIgnoreAttribute : Attribute
    {

    }
    public class TestNHibernate
    {
        public ISessionFactory sessionFactory { get; private set; }

        private ISession sess;

        public class StoreConfiguration : DefaultAutomappingConfiguration
        {
            public override bool ShouldMap(Type type)
            {
                var ignoreByAttribute = type.GetCustomAttributes(typeof(AutomapIgnoreAttribute), true).Any();
                return !ignoreByAttribute && !type.FullName.Contains('+') && type.Namespace == "DojoManagerApi.Entities" && !type.IsEnum    ;
            }
            public override bool ShouldMap(Member member)
            {
                var ignoreByAttribute = member.MemberInfo.GetCustomAttributes(typeof(AutomapIgnoreAttribute), true).Any();
                return !ignoreByAttribute && base.ShouldMap(member);
            }
            public override bool IsComponent(Type type)
            {
                return type == typeof(Address);
            }
        }
        public void Initialize()
        {
            var cfg = new StoreConfiguration();
            var autoMaps =
                AutoMap.AssemblyOf<TestNHibernate>(cfg)
                        .Conventions.Add(DefaultCascade.SaveUpdate());
            sessionFactory = Fluently.Configure()
                            .Database(SQLiteConfiguration.Standard.UsingFile("KenseiDojoDb.db"))
                            .Mappings(m =>
                                     m.AutoMappings.Add(autoMaps)
                                    .ExportTo(@".\")
                                    )
                            .ExposeConfiguration(BuildSchema)
                            .BuildSessionFactory();
            sess = sessionFactory.OpenSession();
        }
        public void Test()
        {
            Initialize();
            Populate();
            PrintPersons();

            //----- printing persons again ----
            var persons = ListPersons();
            foreach (var p in persons)
                Console.WriteLine(p.TestPrint());
        }

        private static void BuildSchema(Configuration config)
        {
            // delete the existing db on each run
            //if (File.Exists(DbFile))
            //    File.Delete(DbFile);

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            if (!File.Exists("KenseiDojoDb.db"))
            {
                new SchemaExport(config).Create(true, true);
            }
            else
            {
                new SchemaUpdate(config).Execute(true, true);
            }
        }

        public IEnumerable<Person> ListPersons()
        {

            var persons = sess.Query<Person>().ToList();

            return persons;


        }

        private DateTime Date(int year, int month) => new DateTime(year, month, 01);
        public void Populate()
        {

            using (var transact = sess.BeginTransaction())
            {
                var p1 = new Person
                {
                    Name = "Gennaro Pepe",
                    Address = new Address() { City = "Napoli", Number = 1, PostCode = "84100", Street = "Spaccanapoli" },
                    BirthDate = new DateTime(1982, 1, 1),
                    EMail = "gennaropepe@kensei.it",
                    PhoneNumber = "166101010"
                };
                p1.AddCertificate(new Certificate { Expiry = DateTime.Now.AddMonths(7), IsCompetitive = true });
                var deb1 = p1.AddSubscription(new Subscription() { StartDate = Date(2021, 10), EndDate = Date(2022, 08), Type = SubscriptionType.Kensei_Dojo_Annual_Association }, 360);
                var deb2 = p1.AddSubscription(new Subscription() { StartDate = Date(2021, 01), EndDate = Date(2022, 01), Type = SubscriptionType.CIK_Annual_Association, Notes = "Prima iscrizione" }, 25);
                deb1.AddPayment(new DebitPayment() { Amount = 360 });

                sess.SaveOrUpdate(deb1);
                sess.SaveOrUpdate(deb2);
                sess.SaveOrUpdate(p1);


                var p2 = new Person
                {
                    Name = "Giulia Spadarella",
                    Address = new Address() { City = "Milano", Number = 1, PostCode = "20160", Street = "via delle spade" },
                    BirthDate = new DateTime(1989, 06, 1),
                    EMail = "gspd@kensei.it",
                    PhoneNumber = "144666333222"
                };
                p2.AddCertificate(new Certificate { Expiry = DateTime.Now.AddMonths(-14), IsCompetitive = false });
                p2.AddCertificate(new Certificate { Expiry = DateTime.Now.AddMonths(-2), IsCompetitive = true });
                var deb3 = p2.AddSubscription(new Subscription() { StartDate = Date(2020, 10), EndDate = Date(2021, 08), Type = SubscriptionType.Kensei_Dojo_Annual_Association }, 400);
                var deb4 = p2.AddSubscription(new Subscription() { StartDate = Date(2021, 09), EndDate = Date(2022, 08), Type = SubscriptionType.Kensei_Dojo_Annual_Association }, 360);
                var deb5 = p2.AddSubscription(new Subscription() { StartDate = Date(2021, 01), EndDate = Date(2022, 01), Type = SubscriptionType.CIK_Annual_Association, Notes = "Prima iscrizione" }, 25);
                deb3.AddPayment(new DebitPayment() { Amount = 400 });
                sess.SaveOrUpdate(deb3);
                sess.SaveOrUpdate(deb4);
                sess.SaveOrUpdate(deb5);

                sess.SaveOrUpdate(p2);

                var of = new CashFlow() { Amount = 100, Date = DateTime.Now, Direction = CashFlowDirection.Out, Notes = "affitto agosto" };
                sess.SaveOrUpdate(of);

                transact.Commit();
            }

        }

        public void PrintPersons()
        { 
            var persons = sess.Query<Person>().ToList();
            foreach (var pers in persons)
                Console.WriteLine(pers.ToString()); 
        }
    }

}
