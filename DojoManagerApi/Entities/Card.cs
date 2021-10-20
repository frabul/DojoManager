using System;

namespace DojoManagerApi.Entities
{
    public enum CardType { Unnown, Kensei, CIK }
    [WrapMe]
    public class Card
    {
        public virtual int Id { get; set; }
        public virtual string CardId { get; set; }
        public virtual DateTime ValidityStartDate { get; set; }
        public virtual DateTime ExpirationDate { get; set; }
        public virtual CardType Type { get; set; }
        public virtual bool Invalidated { get; set; }

        public override string ToString()
        {
            return $"{{ Id:{Id}, Type:{Type}, CardId: {CardId}, Year:{ValidityStartDate:yyyy}, Disabled: {Invalidated} }}";
        }
        //public virtual Person Person { get; set; }
    }
}
