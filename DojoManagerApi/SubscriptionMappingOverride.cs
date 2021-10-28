using DojoManagerApi.Entities;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DojoManagerApi
{
    public class SubscriptionMappingOverride : IAutoMappingOverride<Subscription>
    {
        public void Override(AutoMapping<Subscription> mapping)
        {
            mapping.HasOne(e => e.Debit).Cascade.AllDeleteOrphan();

        }
    }

}
