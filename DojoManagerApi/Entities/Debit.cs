using System.Collections.Generic;

namespace DojoManagerApi.Entities
{
    [WrapMe]
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
            debitPayment.Counterpart = Person.Origin;
            debitPayment.Debit = this;
            this.Payments.Add(debitPayment);
        }
        public override string ToString()
        {
            return $"{{Id:{Id}, Amount: {Amount} }}";
        }
    }
}
