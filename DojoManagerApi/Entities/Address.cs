using Microsoft.EntityFrameworkCore;

namespace DojoManagerApi.Entities
{
    [Owned]
    [WrapMe]
    public record Address
    {
        public Address(Address toCopy)
        {
            this.City = toCopy.City;
            this.Street = toCopy.Street;
            this.Number = toCopy.Number;
            this.PostCode = toCopy.PostCode;
        }
        public virtual string City { get; set; }
        public virtual string Street { get; set; }
        public virtual int Number { get; set; }
        public virtual string PostCode { get; set; }
    }
    [AutomapIgnore]
    public record AddressDetail : Address
    {
        public Address _Origin;
        public object Origin => _Origin;
        public override int Number { get => _Origin.Number; set => _Origin.Number = value; }
        public override string Street { get => _Origin.Street; set => _Origin.Street = value; }
        public AddressDetail(Address a)
        {
            _Origin = a;
        }
    }
}
