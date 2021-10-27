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
        public DbSet<Subject> Subjects { get; set; }
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
                    //.Include(p => p.Debits)
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
            throw new NotImplementedException(); 
        }

        public IEnumerable<Person> ListPersons()
        {
            using (var sess = new DojoManagerContext())
            {
                var persons = sess.Persons.ToList();
                return persons;
            }
        }

        public IEnumerable<Subject> ListSubjects()
        {
            using (var sess = new DojoManagerContext())
            {
                var persons = sess.Subjects.ToList();
                return persons;
            }
        }

    }
}
