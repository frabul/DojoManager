﻿using System;

namespace DojoManagerApi.Entities
{

    [WrapMe]
    public class MembershipCard
    {
        public virtual int Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual string CardId { get; set; }
        public virtual string MemberType { get; set; }
        public virtual DateTime ValidityStartDate { get; set; }
        public virtual DateTime ExpirationDate { get; set; }
        public virtual string Association { get; set; }
        public virtual bool Invalidated { get; set; }
        public virtual string Notes { get; set; }
        public override string ToString()
        {
            return $"{{ Id:{Id}, Type:{Association}, CardId: {CardId}, Year:{ValidityStartDate:yyyy}, Disabled: {Invalidated} }}";
        } 
    }
}
