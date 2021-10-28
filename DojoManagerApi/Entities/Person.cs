using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DojoManagerApi.Entities
{
    

    [WrapMe]
    public class Person : Subject, IEntityWrapper<Person>
    {
        public virtual DateTime BirthDate { get; set; }
        public virtual IList<Certificate> Certificates { get; set; } = new List<Certificate>();
        public virtual IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual IList<Card> Cards { get; set; } = new List<Card>();
        [AutomapIgnore ]
        public virtual Person Origin => this;

        public virtual void RemoveCard(Card c)
        {
            c.Person = null;
            Cards.Remove(c);
        }

        //public virtual IList<Debit> Debits { get; set; } = new List<Debit>();
        public virtual void AddCard(Card card)
        {
            card.Person = Origin;
            Cards.Add(card);
        }

        public virtual void AddCertificate(Certificate certificate)
        {
            certificate.Person = Origin;
            Certificates.Add(certificate);
        }
        public virtual void AddSubscription(Subscription subscription, int cost)
        {
            var deb = new Debit() { Amount = cost  };
            subscription.Debit = deb;
            subscription.Person = Origin;
            Subscriptions.Add(subscription);
            //this.Debits.Add(deb);
            //return deb;
        }
        public virtual void RemoveCertificate(Certificate certificate)
        {
            certificate.Person = null;
            Certificates.Remove(certificate);
        }

        public virtual void RemoveSubscription(Subscription s)
        {
            
            foreach (var p in s.Debit.Payments.ToArray())
            {
                s.Debit.RemovePayment(p);
            }
            //s.Debit.Person = null; 
            s.Debit = null;
            s.Person = null;
            Subscriptions.Remove(s);
        }

        
        public virtual decimal TotalDue()
        {
            return Subscriptions.Select(s => s.Debit.Amount - s.Debit.Payments.Select(p => p.Amount).Sum()).Sum();
        }

        public override string ToString()
        {
            return $"{{ Id: {Id} - Name: {Name} - CertCnt: {Certificates.Count} }}";
        }

        public virtual string PrintData()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Person #{Id} - Name: {Name}, Certificats: {Certificates.Count},  Subscriptions: {Subscriptions.Count}, Cards: {Cards.Count}\n");

            builder.AppendLine("  Certificates");
            foreach (var c in Certificates)
                builder.AppendLine(c.PrintData().PadRight(4));

            builder.AppendLine("  Subscriptions");
            foreach (var s in Subscriptions)
                builder.AppendLine(s.PrintData().PadRight(4));

            builder.AppendLine("  Cards");
            foreach (var c in Cards)
                builder.AppendLine(c.ToString().PadRight(4));
            return builder.ToString();
        }
    }
}
