using Microsoft.EntityFrameworkCore;

namespace DojoManagerApi.Entities
{
    [Owned]
    [WrapMe]
    public class Address
    {
        //public Address(Address toCopy)
        //{
        //    this.City = toCopy.City;
        //    this.Street = toCopy.Street;
        //    this.Number = toCopy.Number;
        //    this.PostCode = toCopy.PostCode;
        //}
        public virtual string City { get; set; }
        public virtual string Street { get; set; }
        public virtual int Number { get; set; }
        public virtual string PostCode { get; set; }
    }
    
  
}
