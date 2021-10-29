using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;
using System.Collections;

namespace DojoManagerApi
{
    internal class NHibernateInterceptor : EmptyInterceptor
    {
        public override SqlString OnPrepareStatement(SqlString sql)
        {
            return base.OnPrepareStatement(sql);
        }
    }
}