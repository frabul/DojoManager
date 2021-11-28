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
        public virtual string PayerName { get; set; }
        public virtual string PayerCode { get; set; }
        public virtual Receipt Receipt { get; set; }

        public DebitPayment()
        {
            Direction = MoneyMovementDirection.In;
        }

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
      
    }
    [WrapMe]
    public class Receipt 
    {
        public virtual int Id { get; set; }
        public virtual int NumberInYear { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual MoneyMovement Movement { get; set; }  
        public Receipt(MoneyMovement movement)
        {
            Movement = movement;
            Date = movement.Date;
        }
        public Receipt( )
        { 
        }

    }



}
