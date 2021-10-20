using System;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class Certificate
    {
        public virtual int Id { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual bool IsCompetitive { get; set; }
        public virtual string ImagePath { get; set; }
       
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        // this is first one '=='
        public static bool operator ==(object obj1, Certificate obj2)
        {
            return obj1.GetHashCode() == obj2.GetHashCode();
        }
        public static bool operator !=(object obj1, Certificate obj2)
        {
            return !(obj1 == obj2);
        }

        public virtual string PrintData()
        {
            return $"{{ Id: {Id}, Competitive: {IsCompetitive}, Expiration:{Expiry:yyyy-MM-dd} }}";
        }
    }
}
