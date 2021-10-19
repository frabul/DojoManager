using System;

namespace DojoManagerApi.Entities
{
    public class Subscription : ISubscription
    {
        public virtual int Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual Debit Debit { get; set; }

        public virtual SubscriptionType Type { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        [AutomapIgnore]
        public virtual decimal Cost { get => Debit.Amount; set => Debit.Amount = value; }

    }


}
