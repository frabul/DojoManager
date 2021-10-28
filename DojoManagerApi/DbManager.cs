using DojoManagerApi.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi
{
    public class DbManager
    {
        public string DbFile { get; }
        public string DbDir { get; }
        public string ImagesDir { get; }
        public ISessionFactory SessionFactory { get; private set; }

        public DbManager(string dbFile)
        {
            DbFile = dbFile;
            DbDir = Path.GetDirectoryName(dbFile);
            var dbName = Path.GetFileNameWithoutExtension(dbFile);
            ImagesDir = Path.Combine(DbDir, dbName + "_images");

        }
        public void Intialize()
        {
            SessionFactory = DbLoader.Load(DbFile);
        }
        public void AddPerson(string name)
        {

        }
        public void RemovePerson(int id)
        {

        }
        public void AddSubscription()
        {

        }
        public void RemoveSubscription()
        {

        }
        public void SetImage(Certificate certificate, string filePath)
        {

        }
        public void AddDebit()
        {

        }
    }
}
