using System;

namespace DojoManagerApi.Entities
{
    public interface ISubscription
    {
        decimal Cost { get; set; }
        Debit Debit { get; set; }
        DateTime EndDate { get; set; }
        int Id { get; set; }
        string Notes { get; set; }
        Person Person { get; set; }
        DateTime StartDate { get; set; }
        SubscriptionType Type { get; set; }
    }
}