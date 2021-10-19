using System.Collections.Generic;

namespace DojoManagerApi.Entities
{
    public interface IDebit
    { 
        decimal Amount { get; set; }
        int Id { get; set; }
        string Notes { get; set; }
        IList<DebitPayment> Payments { get; set; }
        Person Person { get; set; }
        Subscription Subscription { get; set; }

        void AddPayment(DebitPayment debitPayment);
    }
}