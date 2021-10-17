using System;
using System.Collections.Generic;

namespace DojoManagerApi.Entities
{
    public interface IPerson : IJuridicalEntity
    {
        DateTime BirthDate { get; set; }
        IList<Card> Cards { get; set; }
        IList<Certificate> Certificates { get; set; }
        IList<Subscription> Subscriptions { get; set; }
        IList<Debit> Debits { get; set; }   
        void AddCard(Card card);
        void AddCertificate(Certificate certificate);
        Debit AddSubscription(Subscription subscription, int cost);
        string ToString();
    }
}