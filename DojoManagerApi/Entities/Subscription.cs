using System;

namespace DojoManagerApi.Entities
{
    public enum SubscriptionType
    {
        Generic = 1,
        Kensei_Dojo_Annual_Association = 1,
        CIK_Annual_Association,
    }

    [WrapMe]
    public class Subscription
    {
        public virtual int Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual Debit Debit { get; set; }
        public virtual SubscriptionType Type { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        [AutomapIgnore]
        public virtual decimal Cost { get => Debit.Amount; set { if (Debit != null) Debit.Amount = value; } }

        public virtual string PrintData()
        {
            return $"Subscription #{Id}, Type:{Type}, Date:{StartDate}, Notes: {Notes}, Debit: {Debit.ToString()}";
        }
    }


}
