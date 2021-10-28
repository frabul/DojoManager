using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using System.IO;

namespace DojoManagerApi
{
    public static class DbLoader
    {
        public static ISessionFactory Load(string dbFilePath)
        {
            var cfg = new NhibernateAutomappingConfig();
            var autoMaps =
                AutoMap.AssemblyOf<TestNHibernate>(cfg)
                        .Conventions.Add(DefaultCascade.SaveUpdate())
                        .UseOverridesFromAssemblyOf<TestNHibernate>();

            var SessionFactory = Fluently.Configure()
                            .Database(SQLiteConfiguration.Standard.UsingFile(dbFilePath))
                            .Mappings(m =>
                                     m.AutoMappings.Add(autoMaps)
                                    .ExportTo(@".\")
                                    )
                            .ExposeConfiguration(ConfigHandler)
                            .BuildSessionFactory();

            return SessionFactory;
        }
        private static void ConfigHandler(Configuration config)
        {
            // delete the existing db on each run
            //if (File.Exists(DbFile))
            //    File.Delete(DbFile);

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            if (!File.Exists("KenseiDojoDb.db"))
            {
                new SchemaExport(config).Create(true, true);
            }
            else
            {
                new SchemaUpdate(config).Execute(true, true);
            }
            config.SetInterceptor(new SqlStatementInterceptor());
        }

    }
}
