using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class Person : JuridicalEntity
    {
        public virtual DateTime BirthDate { get; set; }
        public virtual IList<Certificate> Certificates { get; set; } = new List<Certificate>();
        public virtual IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual IList<Card> Cards { get; set; } = new List<Card>();
        //public virtual IList<Debit> Debits { get; set; } = new List<Debit>();
        public virtual void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public virtual void AddCertificate(Certificate certificate)
        {
            Certificates.Add(certificate);
        }


        public virtual void AddSubscription(Subscription subscription, int cost)
        {
            var deb = new Debit() { Amount = cost, Person = this, Subscription = subscription };
            subscription.Debit = deb;
            subscription.Person = this;
            Subscriptions.Add(subscription);
            //this.Debits.Add(deb);
            //return deb;
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
