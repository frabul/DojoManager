using System.Collections.Generic;

namespace DojoManagerApi.Entities
{
    [AutomapIgnore]
    public class PersWrapperTest : Person
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

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        } 

        public static bool operator ==(object obj1, PersWrapperTest obj2)
        {
            return obj1.GetHashCode() == obj2.GetHashCode();
        }
        public static bool operator !=(object obj1, PersWrapperTest obj2)
        {
            return !(obj1 == obj2);
        }

    }
}
