using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class JuridicalEntity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string EMail { get; set; }
        public virtual Address Address { get; set; }
        public virtual string PhoneNumber { get; set; }
    }
}
