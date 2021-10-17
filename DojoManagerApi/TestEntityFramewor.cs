using DojoManagerApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi
{
    public class DojoManagerContext : DbContext
    {
        public DbSet<JuridicalEntity> Subjects { get; set; }
        public DbSet<CashFlow> Transactions { get; set; }
        public DbSet<Person> Persons { get; set; }
        public string DbPath { get; private set; }

        public DojoManagerContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = $"./KenseiDojo_ef.db";
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>();
            modelBuilder.Entity<Debit>();
        }

    }


    public class TestEntityFramewor
    {
        public void Test()
        {
            List<Person> persons = null;
            using (var db = new DojoManagerContext())
            {
                // Note: This sample requires the database to be created before running.
                Console.WriteLine($"Creating database path: {db.DbPath}.");
                if (File.Exists(db.DbPath))
                {
                    File.Delete(db.DbPath);
                }
                db.Database.EnsureCreated();

                // Create
                Console.WriteLine("Inserting some data");
                this.Populate(db);

                // Read
                Console.WriteLine("Querying for all subjects");
                persons = db.Persons
                    .OrderBy(b => b.Name)
                    .ToList();

            }
            foreach (var s in persons)
                Console.WriteLine(s.ToString());
            //try query again
            using (var db = new DojoManagerContext())
            {
                Console.WriteLine("Querying for all subjects afert closing db");
                persons = db.Persons
                    .Include(p => p.Certificates)
                    .Include(p => p.Debits)
                    .Include(p => p.Subscriptions)
                    .AsSplitQuery()
                    .OrderBy(b => b.Name)
                    .ToList();
                foreach (var s in persons)
                    Console.WriteLine(s.ToString());
            }
        }
        private DateTime Date(int year, int month) => new DateTime(year, month, 01);

        public void Populate(DojoManagerContext db)
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

           
            db.Add(p1);

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
     

            db.Add(p2);

            var of = new CashFlow() { Amount = 100, Date = DateTime.Now, Direction = CashFlowDirection.Out, Notes = "affitto agosto" };
            db.Add(of);

            db.SaveChanges();


        }

        public IEnumerable<Person> ListPersons()
        {
            using (var sess = new DojoManagerContext())
            {
                var persons = sess.Persons.ToList(); 
                return persons;
            }
        }

        public IEnumerable<JuridicalEntity> ListSubjects()
        {
            using (var sess = new DojoManagerContext())
            {
                var persons = sess.Subjects.ToList();
                return persons;
            }
        }
          
    }
}
