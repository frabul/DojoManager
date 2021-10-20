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
        public virtual bool Invalidated { get; set;  }
        //public virtual Person Person { get; set; }
    }
}
