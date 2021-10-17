using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi.Entities
{
    [Owned]
    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
        public int Number { get; set; }
        public string PostCode { get; set; }
    }
    public class JuridicalEntity : IJuridicalEntity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string EMail { get; set; }
        public virtual Address Address { get; set; }
        public virtual string PhoneNumber { get; set; }
    }

    public class Person : JuridicalEntity, IPerson
    { 
        public virtual DateTime BirthDate { get; set; }
        public virtual IList<Certificate> Certificates { get; set; } = new List<Certificate>();
        public virtual IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual IList<Card> Cards { get; set; }
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

    public class Certificate
    {
        public virtual int Id { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual bool IsCompetitive { get; set; }
        public virtual string ImagePath { get; set; }
    }

    public enum SubscriptionType
    {
        Generic = 1,
        Kensei_Dojo_Annual_Association = 1,
        CIK_Annual_Association,
    }

    public class Subscription
    {
        public virtual int Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual SubscriptionType Type { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
    }

    public class Debit
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual Person Person { get; set; }
        public virtual Subscription Subscription { get; set; }
        public virtual IList<DebitPayment> Payments { get; set; }
        public virtual string Notes { get; set; }

        public Debit()
        {
            Payments = new List<DebitPayment>();
        }
        public virtual void AddPayment(DebitPayment debitPayment)
        {
            debitPayment.Counterpart = Person;
            debitPayment.Debit = this;
            this.Payments.Add(debitPayment);
        }
    }

    public enum CashFlowDirection { In, Out }

    public class CashFlow
    {
        public virtual int Id { get; set; }
        public virtual JuridicalEntity Counterpart { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual CashFlowDirection Direction { get; set; }
    }

    public class DebitPayment : CashFlow
    {
        public virtual Debit Debit { get; set; } //payment is also related to a debit  
        public DebitPayment()
        {
            Direction = CashFlowDirection.In;
        }
    }

    public enum CardType { Unnown, Kensei, CIK}
    public class Card
    {
        public virtual int Id { get; set; }
        public virtual int Number { get; set; }
        public virtual DateTime Expiration { get; set; }
        public virtual CardType Type { get; set; }
        //public virtual Person Person { get; set; }
    }
}
