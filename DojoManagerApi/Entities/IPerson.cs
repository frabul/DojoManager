using System;
using System.Collections.Generic;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public interface IPerson : IJuridicalEntity
    {
        DateTime BirthDate { get; set; }
        IList<Card> Cards { get; set; }
        IList<Certificate> Certificates { get; set; }
        IList<Debit> Debits { get; set; }
        IList<Subscription> Subscriptions { get; set; }

        void AddCard(Card card);
        void AddCertificate(Certificate certificate);
        Debit AddSubscription(Subscription subscription, int cost);
    }
}