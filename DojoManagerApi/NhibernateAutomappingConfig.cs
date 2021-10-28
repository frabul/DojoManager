using DojoManagerApi.Entities;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using System;
using System.Linq;

namespace DojoManagerApi
{
    public class NhibernateAutomappingConfig : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            var ignoreByAttribute = type.GetCustomAttributes(typeof(AutomapIgnoreAttribute), true).Any();
            return !ignoreByAttribute && !type.FullName.Contains('+') && type.Namespace == "DojoManagerApi.Entities" && !type.IsEnum;
        }
        public override bool ShouldMap(Member member)
        {
            var ignoreByAttribute = member.MemberInfo.GetCustomAttributes(typeof(AutomapIgnoreAttribute), true).Any();
            return !ignoreByAttribute && base.ShouldMap(member);
        }
        public override bool IsComponent(Type type)
        {
            return type == typeof(Address);
        }
    }

}
