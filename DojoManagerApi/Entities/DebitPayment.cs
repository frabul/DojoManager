namespace DojoManagerApi.Entities
{
    /// <summary>
    /// A payment that is also related to a debit
    /// </summary>
    [WrapMe]
    public class DebitPayment : MoneyMovement
    {
        public virtual Debit Debit { get; set; }   
        public DebitPayment()
        {
            Direction = CashFlowDirection.In;
        } 
    }
}
