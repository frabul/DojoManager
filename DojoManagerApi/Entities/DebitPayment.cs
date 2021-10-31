using System;

namespace DojoManagerApi.Entities
{
    /// <summary>
    /// A payment that is also related to a debit
    /// </summary>
    [WrapMe]
    public class DebitPayment : MoneyMovement
    {
        public virtual Debit Debit { get; set; }
        public override MoneyMovementDirection Direction
        {
            get => base.Direction;
            set
            {
                if (value != MoneyMovementDirection.In)
                    throw new InvalidOperationException("DebitPayment direction is always IN.");
                base.Direction = value;
            }
        }
        public DebitPayment()
        {
            Direction = MoneyMovementDirection.In;
        }
    }
}
