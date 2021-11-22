using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi.Entities
{
    public enum KendoDegree
    {
        Kyu6 = -6,
        Kyu5,
        Kyu4,
        Kyu3,
        Kyu2,
        Kyu1,
        None = 0,
        Dan1,
        Dan2,
        Dan3,
        Dan4,
        Dan5,
        Dan6,
        Dan7,
        Dan8,
    }

    [WrapMe]
    public class KendoExamination : IEntityWrapper<KendoExamination>
    {
        public virtual int Id { get; set; }
        public virtual Person Person {get;set; }
        public virtual bool Passed { get; set; } = true;
        public virtual KendoDegree DegreeAcquired { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Location { get; set; }
        public virtual string Notes { get; set; }
        [AutomapIgnore]
        public virtual KendoExamination Origin => this;
    }
}
