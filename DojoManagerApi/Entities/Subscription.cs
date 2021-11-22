using System;

namespace DojoManagerApi.Entities
{


    [WrapMe]
    public class Subscription : IEntityWrapper<Subscription>
    {
        [AutomapIgnore]
        public virtual Subscription Origin => this;
        public virtual int Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual Debit Debit { get; set; }
        public virtual string Description { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        [AutomapIgnore]
        public virtual decimal Cost { get => Debit.Amount; set { if (Debit != null) Debit.Amount = value; } }

        public virtual string PrintData()
        {
            return $"Subscription #{Id}, Desc.:{Description}, Date:{StartDate}, Notes: {Notes}, Debit: {Debit.ToString()}";
        }
    }


}
