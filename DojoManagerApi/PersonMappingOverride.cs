using DojoManagerApi.Entities;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DojoManagerApi
{
    [AutomapIgnore]
    public class PersonMappingOverride : IAutoMappingOverride<Person>
    {
        public void Override(AutoMapping<Person> mapping)
        {
            mapping.HasMany(p => p.Certificates).Cascade.AllDeleteOrphan().Inverse();
            mapping.HasMany(p => p.Subscriptions).Cascade.AllDeleteOrphan().Inverse();
            mapping.HasMany(p => p.Cards).Cascade.AllDeleteOrphan().Inverse();
        }
    }

}
