using DojoManagerApi.Entities;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    [AutomapIgnore]
    public class PersonMappingOverride : IAutoMappingOverride<Person>
    {
        public void Override(AutoMapping<Person> mapping)
        {
            mapping.HasMany(p => p.Certificates).Cascade.AllDeleteOrphan().Inverse();
            mapping.HasMany(p => p.Subscriptions).Cascade.AllDeleteOrphan().Inverse();
            mapping.HasMany(p => p.Cards).Cascade.AllDeleteOrphan().Inverse();
        }
    }
    public class SubscriptionMappingOverride : IAutoMappingOverride<Subscription>
    {
        public void Override(AutoMapping<Subscription> mapping)
        {
            mapping.HasOne(e => e.Debit).Cascade.AllDeleteOrphan();

        }
    }
    public class SqlStatementInterceptor : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            //Console.WriteLine();
            //Console.WriteLine(sql.ToString());
            return sql;
        }

    }

    public class StoreConfiguration : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            var ignoreByAttribute = type.GetCustomAttributes(typeof(AutomapIgnoreAttribute), true).Any();
            return !ignoreByAttribute && !type.FullName.Contains('+') && type.Namespace == "DojoManagerApi.Entities" && !type.IsEnum;
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

    [TestClass()]
    public class TestNHibernate
    {
        string[] SubscriptionTypes = new string[] { "Ken Sei Dojo - iscrizione annuale", "CIK - iscrizione annuale" };
        string[] Associations = new string[] { "Ken Sei Dojo", "CIK" };
        public const string DbFile = "KenseiDojoDb.db";

        public void RemovePerson(Person person)
        {

            CurrentSession.Delete((person as IEntityWrapper<Person>).Origin);
            try
            {
                CurrentSession.Flush();
            }
            catch (Exception ex)
            {

            }

        }

        public TestContext TestContext { get; set; }

        public ISessionFactory SessionFactory { get; private set; }
        public ISession CurrentSession { get; private set; }

        private DateTime Date(int year, int month) => new DateTime(year, month, 01);
        List<Person> InitialPersons;
        List<MoneyMovement> InitialCashMovements;

        public TestNHibernate()
        {
            CreateInitialData();
        }
        public void Initialize()
        {
            Close();
            SessionFactory = null;
            var cfg = new StoreConfiguration();
            var autoMaps =
                AutoMap.AssemblyOf<TestNHibernate>(cfg)
                        .Conventions.Add(DefaultCascade.SaveUpdate())
                        .UseOverridesFromAssemblyOf<TestNHibernate>();

            SessionFactory = Fluently.Configure()
                            .Database(SQLiteConfiguration.Standard.UsingFile(DbFile))
                            .Mappings(m =>
                                     m.AutoMappings.Add(autoMaps)
                                    .ExportTo(@".\")
                                    )
                            .ExposeConfiguration(ConfigHandler)
                            .BuildSessionFactory();

            CurrentSession = SessionFactory.OpenSession();
        }
        private void CreateInitialData()
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
            p1.AddSubscription(new Subscription() { StartDate = Date(2021, 10), EndDate = Date(2022, 08), Description = SubscriptionTypes[0], Notes = "Sec iscrizione" }, 360);
            p1.AddSubscription(new Subscription() { StartDate = Date(2020, 01), EndDate = Date(2021, 01), Description = SubscriptionTypes[1], Notes = "Prima iscrizione" }, 25);
            p1.Subscriptions[0].Debit.AddPayment(360, p1.Subscriptions[0].StartDate.AddDays(1), p1.Origin);
            p1.AddCard(new MembershipCard() { CardId = "1111", Association = Associations[0], Invalidated = true });
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
            p2.AddSubscription(new Subscription() { StartDate = Date(2020, 10), EndDate = Date(2021, 08), Description = SubscriptionTypes[0], Notes = "Prima iscrizione" }, 400);
            p2.AddSubscription(new Subscription() { StartDate = Date(2021, 09), EndDate = Date(2022, 08), Description = SubscriptionTypes[1], Notes = "Sec iscrizione" }, 360);
            p2.AddSubscription(new Subscription() { StartDate = Date(2021, 01), EndDate = Date(2022, 01), Description = SubscriptionTypes[1], Notes = "CIK" }, 25);
            p2.Subscriptions[1].Debit.AddPayment(360, p2.Subscriptions[1].StartDate.AddDays(1), p2.Origin);

            p2.AddCard(new MembershipCard() { CardId = "22222", Association = Associations[1], Invalidated = true });
            InitialPersons = new() { p1, p2 };

            InitialCashMovements = new()
            {
                new MoneyMovement() { Amount = 100, Date = DateTime.Now, Direction = MoneyMovementDirection.Out, Notes = "affitto agosto" }
            };
        }



        private static void ConfigHandler(Configuration config)
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
            config.SetInterceptor(new SqlStatementInterceptor());
        }

        public void Close()
        {
            CurrentSession?.Dispose();
            SessionFactory?.Dispose();
            CurrentSession = null;
            SessionFactory = null;
        }

        public void DeleteDb()
        {
            if (File.Exists(DbFile))
                File.Delete(DbFile);
        }

        void Mod1(Person p)
        {
            p.Name = "spastico";
            p.BirthDate = new DateTime(1987, 1, 1);
            p.RemoveCertificate(p.Certificates[0]);
            p.AddCertificate(new Certificate() { Expiry = DateTime.Now.AddYears(1), IsCompetitive = false });
            p.AddCard(new MembershipCard() { CardId = "asd asd asd", ValidityStartDate = DateTime.Now });
            p.RemoveCard(p.Cards[0]);
            p.AddSubscription(new Subscription() { Description = SubscriptionTypes[1] }, 156);
        }
        [TestMethod]
        public void Test1()
        {
            DeleteDb();
            Initialize();
            Populate();
            PrintPersons();
            Flush();

            //reinitialize
            Initialize();
            var persons = ListPersons();
            CompareToInitialSet(persons);
            Flush();
            Initialize();
        }
        [TestMethod]
        public void Test2()
        {
            Close();
            DeleteDb();
            Initialize();
            Populate();



            Close();
            Initialize();

            var persons = ListPersons();//.Select(p => EntityWrapper.Wrap(p) as Person).ToArray();
            Mod1(persons[0]);
            Mod1(InitialPersons[0]);

            Flush();
            Initialize();

            var newPersons = ListPersons();
            CompareToInitialSet(ListPersons());
            CompareToInitialSet(ListPersons().Select(p => EntityWrapper.Wrap(p) as Person).ToList());
        }
        [TestMethod]
        public void Test3()
        {

            DeleteDb();
            Initialize();
            Populate();
            Close();

            Initialize();
            var persons = ListPersons().Select(p => EntityWrapper.Wrap(p) as Person).ToArray();
            Mod1(persons[0]);
            Mod1(InitialPersons[0]);
            Flush();
            Close();

            Initialize();

            CompareToInitialSet(ListPersons());
            CompareToInitialSet(ListPersons().Select(p => EntityWrapper.Wrap(p) as Person).ToList());
        }

        [TestMethod]
        public void Test_Deletions()
        {

            DeleteDb();
            Initialize();
            Populate();
            Close();

            Initialize();
            var person = CurrentSession.Query<Person>().First();
            var personId = person.Id;
            var certRemoved = person.Certificates.First();
            var cardRemoved = person.Cards.First();
            var subRemoved = person.Subscriptions.First();
            var debRemoved = subRemoved.Debit.Id;
            var payments = subRemoved.Debit.Payments.ToArray();
            person.RemoveCard(cardRemoved);
            person.RemoveCertificate(certRemoved);
            person.RemoveSubscription(subRemoved);
            Flush();
            Close();

            Initialize();
            var person2 = CurrentSession.Get<Person>(person.Id);
            var cert2 = CurrentSession.Get<Certificate>(certRemoved.Id);
            var card2 = CurrentSession.Get<MembershipCard>(cardRemoved.Id);
            var sub2 = CurrentSession.Get<Subscription>(subRemoved.Id);
            var deb2 = CurrentSession.Get<Debit>(debRemoved);
            Assert.IsNotNull(person2);
            Assert.IsNull(cert2);
            Assert.IsNull(card2);
            Assert.IsNull(sub2);
            Assert.IsNull(deb2);
            foreach (var pay in payments)
            {
                Assert.IsNotNull(CurrentSession.Get<DebitPayment>(pay.Id));
            }

        }

        private void CompareToInitialSet(List<Person> persons)
        {
            foreach (var p in persons)
                Console.WriteLine(p.PrintData());
            Assert.IsTrue(persons.Count == 2);

            for (int i = 0; i < InitialPersons.Count; i++)
            {
                var pi = InitialPersons[i];
                var pn = persons[i];
                //var pdb = pn.Subscriptions[0].Debit.Person;
                var debit_0_0 = pn.Subscriptions[0].Debit;
                var payments = debit_0_0.Payments;
                if (pi.TotalDue() != pn.TotalDue())
                { }
                Assert.AreEqual(pi.TotalDue(), pn.TotalDue());
                Assert.IsTrue(pi.Subscriptions.Count == pn.Subscriptions.Count);
                Assert.IsTrue(pi.Certificates.Count == pn.Certificates.Count);
                Assert.IsTrue(pi.Cards.Count == pn.Cards.Count);
                for (int s = 0; s < pi.Subscriptions.Count; s++)
                {
                    Assert.AreEqual(pi.Subscriptions[s].Description, pn.Subscriptions[s].Description);
                    Assert.IsTrue(pi.Subscriptions[s].Debit.Amount == pn.Subscriptions[s].Debit.Amount);
                    for (int d = 0; d < pi.Subscriptions[s].Debit.Payments.Count; d++)
                        Assert.IsTrue(pi.Subscriptions[s].Debit.Payments[d].Amount == pn.Subscriptions[s].Debit.Payments[d].Amount);

                }
            }
        }

        public List<Person> ListPersons()
        {
            var persons = CurrentSession.Query<Person>().ToList();
            return persons;
        }

        public void Populate()
        {
            CreateInitialData();
            using var transact = CurrentSession.BeginTransaction();

            foreach (var p in InitialPersons)
                CurrentSession.Save(p);

            foreach (var cf in InitialCashMovements)
                CurrentSession.Save(cf);
            transact.Commit();
        }

        public void PrintPersons()
        {
            var persons = CurrentSession.Query<Person>().ToList();
            foreach (var pers in persons)
                Console.WriteLine(pers.PrintData());
        }

        public void Flush()
        {
            CurrentSession.Flush();
        }

        public Person AddNewPerson()
        {
            var p = new Person() { Name = "Jonhn Doe" };
            CurrentSession.Save(p);
            return p;
        }

        public List<MoneyMovement> GetMoneyMovements()
        {
            return CurrentSession.Query<MoneyMovement>().ToList();
        }
    }

}
