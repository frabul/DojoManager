using System;
using System.Collections.Generic;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class Debit : IEntityWrapper<Debit>
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        //public virtual Person Person { get; set; }

        public virtual IList<DebitPayment> Payments { get; set; }
        [AutomapIgnore]
        public virtual Debit Origin => this;
        public Debit()
        {
            Payments = new List<DebitPayment>();
        }
        public virtual DebitPayment AddPayment(decimal amount, DateTime date, Subject payer)
        {
            var payment = new DebitPayment()
            {
                Amount = amount,
                Debit = this.Origin,
                Counterpart = payer,
                Date = date,
                Notes = ""
            };
            this.Payments.Add(payment);
            return payment;
        }
        public virtual void RemovePayment(DebitPayment debitPayment)
        {
            debitPayment.Debit = null;
            var ok = this.Payments.Remove(debitPayment);
        }
        public override string ToString()
        {
            return $"{{Id:{Id}, Amount: {Amount} }}";
        }
    }
}
