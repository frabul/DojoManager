using System;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class Certificate
    {
        public virtual int Id { get; set; }
        public virtual Person Person { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual bool IsCompetitive { get; set; }
        public virtual string ImagePath { get; set; }
        public virtual string Notes { get; set; }

        public virtual string PrintData()
        {
            return $"{{ Id: {Id}, Competitive: {IsCompetitive}, Expiration:{Expiry:yyyy-MM-dd} }}";
        }
    }
}
