using DojoManagerApi.Entities;
using FluentNHibernate.Automapping;
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


    [TestClass()]

    public class TestNHibernate
    {

        string[] SubscriptionTypes = new string[] { 
            "Ken Sei Dojo - quota associativa (aaaa/aaaa)",
            "CIK - quota associativa (aaaa/aaaa)"
        };
        string[] Associations = new string[] { "Ken Sei Dojo", "CIK" };
        public const string DbFile = "KenseiDojoDb.db";

        public static void PopulateDb(DbManager db)
        {
            var tester = new TestNHibernate();
            db.AddEntities(tester.InitialCashMovements.Select(m => EntityWrapper.Wrap(m)));
            db.AddEntities(tester.InitialPersons);

        }

        public TestContext TestContext { get; set; }
        private DateTime Date(int year, int month) => new DateTime(year, month, 01);
        List<Person> InitialPersons;
        List<MoneyMovement> InitialCashMovements;
        DbManager db;
        public TestNHibernate()
        {

            CreateInitialData();
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
            Populate();
            PrintPersons();
            db.Save();

            //reinitialize
            db.Close();
            db.Load();
            var persons = db.ListPeople();
            CompareToInitialSet(persons);


        }
        [TestMethod]
        public void Test2()
        {
            Populate();



            db.Close();
            db.Load();

            var persons = db.ListPeople();//.Select(p => EntityWrapper.Wrap(p) as Person).ToArray();
            Mod1(persons[0]);
            Mod1(InitialPersons[0]);

            db.Save();
            db.Close();
            db.Load();

            var newPersons = db.ListPeople();
            CompareToInitialSet(db.ListPeople());
            CompareToInitialSet(db.ListPeople().Select(p => EntityWrapper.Wrap(p) as Person).ToList());
        }
        [TestMethod]
        public void Test3()
        {
            Populate();
            db.Close();

            db.Load();
            var persons = db.ListPeople().Select(p => EntityWrapper.Wrap(p) as Person).ToArray();
            Mod1(persons[0]);
            Mod1(InitialPersons[0]);
            db.Save();
            db.Close();

            db.Load();

            CompareToInitialSet(db.ListPeople());
            CompareToInitialSet(db.ListPeople().Select(p => EntityWrapper.Wrap(p) as Person).ToList());
        }

        [TestMethod]
        public void Test_Deletions()
        {
            Populate();
            db.Close();

            db.Load();
            var person = db.ListPeople().First();
            var personId = person.Id;
            var certRemoved = person.Certificates.First();
            var cardRemoved = person.Cards.First();
            var subRemoved = person.Subscriptions.First();
            var debRemoved = subRemoved.Debit.Id;
            var payments = subRemoved.Debit.Payments.ToArray();
            person.RemoveCard(cardRemoved);
            person.RemoveCertificate(certRemoved);
            person.RemoveSubscription(subRemoved);
            db.Save();
            db.Close();

            db.Load();
            var person2 = db.GetEntity<Person>(person.Id);
            var cert2 = db.GetEntity<Certificate>(certRemoved.Id);
            var card2 = db.GetEntity<MembershipCard>(cardRemoved.Id);
            var sub2 = db.GetEntity<Subscription>(subRemoved.Id);
            var deb2 = db.GetEntity<Debit>(debRemoved);
            Assert.IsNotNull(person2);
            Assert.IsNull(cert2);
            Assert.IsNull(card2);
            Assert.IsNull(sub2);
            Assert.IsNull(deb2);
            foreach (var pay in payments)
            {
                Assert.IsNotNull(db.GetEntity<DebitPayment>(pay.Id));
            }

        }
        [TestCleanup]
        public void CloseAndClear()
        {
            db.Close();
            db.ClearDatabase();
        }

        [TestInitialize]
        public void LoadBlankDb()
        {
            var docsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            db = new DbManager("TestDb", docsDir);
            db.ClearDatabase();
            db.Load();
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

        public void Populate()
        {
            CreateInitialData();
            db.AddEntities(InitialPersons.Cast<object>().Concat(InitialCashMovements));
        }

        public void PrintPersons()
        {
            var persons = db.ListPeople();
            foreach (var pers in persons)
                Console.WriteLine(pers.PrintData());
        }
    }
}
