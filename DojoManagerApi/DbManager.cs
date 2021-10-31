using DojoManagerApi.Entities;
using NHibernate;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        public bool IsOpen => CurrentSession != null;

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

        public List<T> ListEntities<T>(Func<IQueryable<T>, IQueryable<T>> query)
        {
            var q = query.Invoke(CurrentSession.Query<T>());
            return q.ToList();
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
        public Subject AddNewSubject(string name)
        {
            return ExecuteWithSession(() =>
            {
                var p = new Subject() { Name = name };
                CurrentSession.Save(p);
                return p;
            });
        }
        public void RemoveSubscription()
        {

        }
        public void SetImage(Person person, string imgPath)
        {
            var img = Image.Load(imgPath);
            SetImage(person, img);
        }
        public void SetImage(Person person, Image img)
        {
            var destFileName = $"Person_Picture_{person.Id}.png";
            var maxHeight = 400;
            var maxWidth = 300;
            var heightRatio = (float)img.Height / maxHeight;
            var widthRatio = (float)img.Width / maxWidth;
            if (heightRatio > 1 || widthRatio > 1)
            {
                var reduction = 1f;
                if (heightRatio > widthRatio)
                    reduction = 1f / heightRatio;
                else
                    reduction = 1f / widthRatio;
                img.Mutate(x => x.Resize((int)(reduction * img.Width), (int)(reduction * img.Height)));

            }
            if (File.Exists(destFileName))
                File.Delete(destFileName);
            img.Save(Path.Combine(ImagesDir, destFileName));
            person.PictureFileName = destFileName;
        }
        public void SetImage(Certificate certificate, string filePath)
        {
            string imageFileName = Path.GetFileName(filePath);
            string imageFileDir = Path.GetDirectoryName(filePath);
            //assure that file is in images directory
            if (!Path.Equals(imageFileDir, ImagesDir))
            {
                string extension = Path.GetExtension(filePath);
                var n = certificate.Person.Name.Replace(' ', '_');
                var destFileName = $"Certificate_{n}_{certificate.Id}.{extension}";
                var finalImagePath = Path.Combine(ImagesDir, destFileName);
                File.Copy(filePath, finalImagePath, true);
                imageFileName = Path.GetFileName(finalImagePath);
            }
            certificate.ImageFileName = imageFileName;
        }

        public string GetImagePath(Certificate certificate)
        {
            if ( !string.IsNullOrEmpty(certificate.ImageFileName))
                return Path.Combine(ImagesDir, certificate.ImageFileName);
            else
                return null;
        }

        public byte[] GetImageBytes(Person person)
        {
            if (!string.IsNullOrEmpty(person.PictureFileName))
            {


                var img = Image.Load<Rgba32>(Path.Combine(ImagesDir, person.PictureFileName));
                //var ok = img.TryGetSinglePixelSpan(out Span<Rgba32> span );
                //if (ok)
                //{
                //    byte[] rgbaBytes = MemoryMarshal.AsBytes(span ).ToArray();
                //    return rgbaBytes;
                //}
                using (MemoryStream ms = new MemoryStream())
                {
                    img.SaveAsPng(ms);
                    var buffer = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            return new byte[0];
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
        public MoneyMovement AddNewMovement( Subject subject)
        {
            if (subject is IEntityWrapper<Subject> entityWrapper)
                subject = entityWrapper.Origin;
            var mov = new MoneyMovement() {  Counterpart = subject };
            CurrentSession.Save(mov);
            return mov;
        }
        public List<Subject> ListSubjects(string nameFilter = null)
        {
            return ExecuteWithSession(() =>
            {
                var q = CurrentSession.Query<Subject>();
                if (nameFilter != null)
                    q = q.Where(p => p.Name.Contains(nameFilter));
                return q.ToList();
            });
        }

        public List<Person> ListPeople(string nameFilter = null)
        {
            return ExecuteWithSession(() =>
            {
                var q = CurrentSession.Query<Person>();
                if (nameFilter != null)
                    q = q.Where(p => p.Name.Contains(nameFilter));
                return q.ToList();
            });
        }


        public List<MoneyMovement> ListMovements(DateTime startTime, DateTime endTime, int subjectId)
        {
            return ExecuteWithSession(() =>
            {
                var persons = CurrentSession
                    .Query<MoneyMovement>()
                    .Where(e => e.Date >= startTime && e.Date <= endTime && e.Counterpart.Id == subjectId)
                    .ToList();
                return persons;
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
