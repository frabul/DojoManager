using System;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class MoneyMovement
    {
        public virtual int Id { get; set; }
        public virtual Subject Counterpart { get; set; }
        public virtual decimal Amount { get; set; } 
        public virtual DateTime Date { get; set; } = DateTime.Now;
        public virtual MoneyMovementDirection Direction { get; set; }
        public virtual string Notes { get; set; }

        [AutomapIgnore]
        public virtual decimal AmountSigned => Direction == MoneyMovementDirection.In ? Amount : -Amount;
    }

    public enum MoneyMovementDirection { In, Out }
}
