using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerGui.ViewModels
{
    public class VM_Address : INotifyPropertyChanged
    {
        public string City { get; set; }
        public string Street { get; set; }
        public int Number { get; set; }
        public string PostCode { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public class VM_JuridicalEntity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string EMail { get; set; }
        public virtual VM_Address Address { get; set; }
        public virtual string PhoneNumber { get; set; }
    }

    public class VM_Person : VM_JuridicalEntity
    {
        public VM_Person()
        {
            Certificates = new ObservableCollection<VM_Certificate>();
            Subscriptions = new ObservableCollection<VM_Subscription>();
        }
        public virtual DateTime BirthDate { get; set; }
        public virtual IList<VM_Certificate> Certificates { get; set; }
        public virtual IList<VM_Subscription> Subscriptions { get; set; }
        public virtual IList<VM_Card> Cards { get; set; }

        public virtual void AddCard(VM_Card card)
        {
            Cards.Add(card);
        }

        public virtual void AddCertificate(VM_Certificate certificate)
        {
            Certificates.Add(certificate);
        }


        public virtual VM_Debit AddSubscription(VM_Subscription subscription, int cost)
        {
            Subscriptions.Add(subscription);
            subscription.Person = this;
            return new VM_Debit() { Amount = cost, Person = this, Subscription = subscription };
        }

        public override string ToString()
        {
            return $"{{ Id: {Id} - Name: {Name} - CertCnt: {Certificates.Count} }}";
        }

    }
    public class VM_Certificate
    {
        public virtual int Id { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual bool IsCompetitive { get; set; }
        public virtual string ImagePath { get; set; }
    }
    public class VM_Subscription
    {
        public virtual int Id { get; set; }
        public virtual VM_Person Person { get; set; }
        public virtual DojoManagerApi.Entities.SubscriptionType Type { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
    }

    public class VM_Debit
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual VM_Person Person { get; set; }
        public virtual VM_Subscription Subscription { get; set; }
        public virtual IList<VM_DebitPayment> Payments { get; set; }
        public virtual string Notes { get; set; }

        public VM_Debit()
        {
            Payments = new List<VM_DebitPayment>();
        }
        public virtual void AddPayment(VM_DebitPayment debitPayment)
        {
            debitPayment.Counterpart = Person;
            debitPayment.Debit = this;
            this.Payments.Add(debitPayment);
        }
    }
    public class VM_CashFlow
    {
        public virtual int Id { get; set; }
        public virtual VM_JuridicalEntity Counterpart { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual DojoManagerApi.Entities.CashFlowDirection Direction { get; set; }
    }

    public class VM_DebitPayment : VM_CashFlow
    {
        public virtual VM_Debit Debit { get; set; } //payment is also related to a debit  
        public VM_DebitPayment()
        {
            Direction = DojoManagerApi.Entities.CashFlowDirection.In;
        }
    }

    public enum CardType { Unnown, Kensei, CIK }
    public class VM_Card
    {
        public virtual int Id { get; set; }
        public virtual int Number { get; set; }
        public virtual DateTime Expiration { get; set; }
        public virtual CardType Type { get; set; }
        //public virtual Person Person { get; set; }
    }
}
