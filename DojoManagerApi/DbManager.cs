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
        public string DbRoot { get; }
        public string ImagesDir { get; }
        public string BackupsDir { get; }

        private ISessionFactory SessionFactory;
        private ISession CurrentSession;
        private object SessionLocker = new object();

        public DbManager(string dbName, string dbDir)
        {
            DbRoot = Path.Combine(dbDir, dbName);
            DbFile = Path.Combine(DbRoot, dbName + ".sqlite");
            ImagesDir = Path.Combine(DbRoot, "Images");
            BackupsDir = Path.Combine(DbRoot, "Backups");
            var dirs = new string[] { DbRoot, ImagesDir, BackupsDir };
            foreach (var dir in dirs)
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
        }

        public void Load()
        {
            lock (SessionLocker)
            {
                if (CurrentSession != null)
                    throw new Exception("Session is already open.");
                SessionFactory = DbLoader.Load(DbFile);
                CurrentSession = SessionFactory.OpenSession();
            }

        }

        public void Close()
        {
            ExecuteWithSession(() =>
            {
                CurrentSession?.Dispose();
                SessionFactory?.Dispose();
                CurrentSession = null;
                SessionFactory = null;
            });
        }

        public void Save()
        {
            ExecuteWithSession(() =>
            {
                if (CurrentSession.IsDirty())
                {
                    CreateBackUp();
                    CurrentSession.Flush();
                }
            });
        }

        private void CreateBackUp()
        {
            //TODO
        }


        public void ClearDatabase()
        {
            lock (SessionLocker)
            {
                if (CurrentSession != null)
                    Close();

                if (File.Exists(DbFile))
                    File.Delete(DbFile);
                //todo delete images and history
            }
        }

        public void Delete(object entity)
        {
            ExecuteWithSession(() =>
            {
                if (entity is IEntityWrapper<object> entityWrapper)
                    entity = entityWrapper.Origin;
                CurrentSession.Delete(entity);
            });
        }

        public Person AddNewPerson(string name)
        {
            return ExecuteWithSession(() =>
            {
                var p = new Person() { Name = name };
                CurrentSession.Save(p);
                return p;
            });
        }

        public void RemoveSubscription()
        {

        }

        public void SetImage(Certificate certificate, string filePath)
        {

        }

        public void GetImagePath(Certificate certificate)
        {

        }

        public void AddEntities(IEnumerable<object> entities)
        {
            ExecuteWithSession(() =>
            {
                using var transact = CurrentSession.BeginTransaction();
                foreach (var e in entities)
                {
                    if (e is IEntityWrapper<object> entityWrapper)
                        CurrentSession.Save(entityWrapper.Origin);
                    else
                        CurrentSession.Save(e);
                }
                transact.Commit();
            });
        }

        public List<Person> ListPersons(string nameFilter = null)
        {
            return ExecuteWithSession(() =>
            {
                var q = CurrentSession.Query<Person>();
                if (nameFilter != null)
                    q = q.Where(p => p.Name.Contains(nameFilter));
                return q.ToList();
            });
        }

        public List<MoneyMovement> ListMovements(DateTime startTime, DateTime endTime)
        {
            return ExecuteWithSession(() =>
            {
                var persons = CurrentSession
                    .Query<MoneyMovement>()
                    .Where(e => e.Date >= startTime && e.Date <= endTime)
                    .ToList();
                return persons;
            });
        }
        public List<MoneyMovement> ListMovements()
        {
            return ExecuteWithSession(() =>
            {
                var persons = CurrentSession
                    .Query<MoneyMovement>()
                    .ToList();
                return persons;
            });
        }
        public T GetEntity<T>(int id)
        {
            return ExecuteWithSession(() => CurrentSession.Get<T>(id));
        }

        private void ExecuteWithSession(Action p)
        {
            CheckSessionNotNull();
            lock (SessionLocker)
                p.Invoke();
        }

        private T ExecuteWithSession<T>(Func<T> p)
        {
            CheckSessionNotNull();
            lock (SessionLocker)
                return p.Invoke();
        }
        private void CheckSessionNotNull()
        {
            if (CurrentSession == null)
                throw new Exception("No session.");
        }


    }
}
