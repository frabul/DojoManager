using System;
using System.Collections.Generic;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class Certificate
    {
        public virtual int Id { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual bool IsCompetitive { get; set; }
        public virtual string ImagePath { get; set; }
        public virtual string Notes { get; set; }
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
    [AutomapIgnore]
    public class PersWrapper : Person
    {
        public Person Origin { get; set; }
        private LinkedObservableCollection<Certificate> _certificates;
        public override IList<Certificate> Certificates
        {
            get
            {
                if (_certificates == null)
                {
                    _certificates = new LinkedObservableCollection<Certificate>(Origin.Certificates);
                }
                return _certificates;
            }
            set => base.Certificates = value;
        }
    }
}
