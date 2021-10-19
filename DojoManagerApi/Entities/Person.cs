using System;
using System.Collections.Generic;
using System.Linq;

namespace DojoManagerApi.Entities
{
    public class Person : JuridicalEntity, IPerson
    {
        public virtual DateTime BirthDate { get; set; }
        public virtual IList<Certificate> Certificates { get; set; } = new List<Certificate>();
        public virtual IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual IList<Card> Cards { get; set; } = new List<Card>();
        public virtual IList<Debit> Debits { get; set; } = new List<Debit>();
        public virtual void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public virtual void AddCertificate(Certificate certificate)
        {
            Certificates.Add(certificate);
        }


        public virtual Debit AddSubscription(Subscription subscription, int cost)
        {
            Subscriptions.Add(subscription);
            subscription.Person = this;
            var deb = new Debit() { Amount = cost, Person = this, Subscription = subscription };
            this.Debits.Add(deb);
            return deb;
        }

        public override string ToString()
        {
            return $"{{ Id: {Id} - Name: {Name} - CertCnt: {Certificates.Count} }}";
        }

        public virtual string TestPrint()
        {
            return $"{{ Id: {Id} - Name: {Name} - CertCnt: {Certificates.Count} - d: {Debits.Count} - " +
                $"p {Debits.SelectMany(d => d.Payments).Count()} - " +
                $" subs {Subscriptions.Count} - {Cards.Count}";

        }
    }


}
