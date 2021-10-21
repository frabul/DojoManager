namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class DebitPayment : CashFlow
    {
        public virtual Debit Debit { get; set; } //payment is also related to a debit  
        public DebitPayment()
        {
            Direction = CashFlowDirection.In;
        } 
    }
}
