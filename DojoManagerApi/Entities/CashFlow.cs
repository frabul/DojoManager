using System;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class CashFlow
    {
        public virtual int Id { get; set; }
        public virtual JuridicalEntity Counterpart { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual CashFlowDirection Direction { get; set; }
    }

    public enum CashFlowDirection { In, Out }
}
