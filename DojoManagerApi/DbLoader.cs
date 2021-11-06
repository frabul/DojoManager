using DojoManagerApi.Entities;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.IO;

namespace DojoManagerApi
{
    /// <summary>
    /// Todo refreshare tutti quando si cancella o aggiunge oggetti base
    /// todo fare aggiunte movimenti
    /// </summary>
    public static class DbLoader
    {
        public static string DbFilePath { get; private set; }

        public static ISessionFactory Load(string dbFilePath)
        {
            DbFilePath = dbFilePath;
            var cfg = new NhibernateAutomappingConfig();
            var autoMaps =
                AutoMap.AssemblyOf<Person>(cfg)
                        .Conventions.Add(DefaultCascade.SaveUpdate())
                        .UseOverridesFromAssemblyOf<Person>();

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
            if (!File.Exists(DbFilePath))
            {
                new SchemaExport(config).Create(false, true);
            }
            else
            {
                new SchemaUpdate(config).Execute(true, true);
            }
            config.SetInterceptor(new NHibernateInterceptor());
        }

    }
}
