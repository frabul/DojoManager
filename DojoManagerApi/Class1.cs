using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.IO;

namespace DojoManagerApi.Test1
{
    public class DatabaseManager
    {
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile("KenseiDojoDb.db"))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DatabaseManager>())
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
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
                new SchemaExport(config)
                  .Create(false, true);
            }
            else
            {
                new SchemaUpdate(config).Execute(true, true);
            }
        }

        public void Test()
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    Person p1 = new Person { Name = "Mastro Geppetto", Id = 0 };
                    Person p2 = new Person { Name = "Gennaro Pepe" };
                    var cert1 = new Certificate() { Expiry = DateTime.Now.AddMonths(12) };
                    var cert2 = new Certificate { Expiry = DateTime.Now.AddMonths(-2) };
                    p1.AddCertificate(cert2);
                    p1.AddCertificate(cert1);

                    // save both stores, this saves everything else via cascading
                    session.SaveOrUpdate(p1);
                    session.SaveOrUpdate(p2);

                    transaction.Commit();
                }
                // retrive all stores and display them
                using (session.BeginTransaction())
                {
                    var persons = session.CreateCriteria(typeof(Person))
                      .List<Person>();

                    foreach (var adept in persons)
                    {
                        Console.WriteLine(adept.ToString());
                    }
                }
            }
            Console.ReadKey();
        }

    }
    public class Entity
    {
        public int Id { get; set; }
    }
    public class Person
    {
        public Person()
        {
            Certificates = new List<Certificate>();
        }
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string ResidenceCity { get; set; }
        public virtual string ResidenceAddress { get; set; }
        public virtual string EMail { get; set; }

        public virtual IList<Certificate> Certificates { get; set; }

        public virtual void AddCertificate(Certificate certificate)
        {
            Certificates.Add(certificate);
        }

        public override string ToString()
        {
            return $"{{ Id: {Id} - Name: {Name} - CertCnt: {Certificates.Count} }}";
        }

        public class Map : ClassMap<Person>
        {
            public Map()
            {
                Id(x => x.Id); 
                Map(x => x.Name).Unique();
                Map(x => x.BirthDate);
                Map(x => x.EMail);
                Map(x => x.PhoneNumber);
                Map(x => x.ResidenceAddress);
                Map(x => x.ResidenceCity);
                HasMany(x => x.Certificates).Cascade.All();
            }
        }
    }

    public class Certificate
    {
        public virtual int Id { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual bool Competitive { get; set; }
        public virtual string ImagePath { get; set; }
        public class Map : ClassMap<Certificate>
        {
            public Map()
            {
                Id(x => x.Id);
                Map(x => x.Expiry);
                Map(x => x.Competitive);
                Map(x => x.ImagePath);
            }
        }
    }

    public enum SubscriptionType
    {
        KenseiDojo,
        CIK,
    }

    public class Subscription
    {
        public virtual int Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual SubscriptionType Type { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public class Map : ClassMap<Subscription>
        {
            public Map()
            {
                Id(x => x.Id);
                Map(x => x.Type).CustomType<SubscriptionType>();
                Map(x => x.Notes);
                Map(x => x.StartDate);
                Map(x => x.EndDate);
                this.References(x => x.Person);
            }
        }
    }


    public class Debit
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual Person Person { get; set; }
        public virtual Subscription Subscription { get; set; }
        public virtual IList<Payment> Payments { get; set; }
        public virtual string Notes { get; set; }
        public class Map : ClassMap<Debit>
        {
            public Map()
            {
                Id(x => x.Id);
                Map(x => x.Amount);
                Map(x => x.Notes);
                this.HasMany(x => x.Payments).Inverse().Cascade.All();
                this.References(x => x.Subscription);
                this.References(x => x.Person);
            }
        }
    }

    public class Payment
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Notes { get; set; }
        public virtual Person Person { get; set; }
        public virtual DateTime PaymentDate { get; set; }
        public virtual Debit Debit { get; set; }
        public class Map : ClassMap<Payment>
        {
            public Map()
            {
                Id(x => x.Id);
                Map(x => x.Amount);
                Map(x => x.Notes);
                this.References(x => x.Person);
                this.References(x => x.Debit);
            }
        }

    }

}