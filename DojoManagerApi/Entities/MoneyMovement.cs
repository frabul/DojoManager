using System;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class MoneyMovement
    {
        public virtual int Id { get; set; }
        public virtual Subject Counterpart { get; set; }
        public virtual decimal Amount { get; set; } 
        public virtual DateTime Date { get; set; }
        public virtual CashFlowDirection Direction { get; set; }
        public virtual string Notes { get; set; }
    }

    public enum CashFlowDirection { In, Out }
}
