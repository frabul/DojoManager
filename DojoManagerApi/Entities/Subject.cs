using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi.Entities
{
    [WrapMe]
    public class Subject
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string EMail { get; set; }
        public virtual Address Address { get; set; } = new Address();
        public virtual string PhoneNumber { get; set; }
        public virtual string Notes { get; set; }

        public virtual IList<MoneyMovement> Movements { get; set; } = new List<MoneyMovement>();
    }
}
