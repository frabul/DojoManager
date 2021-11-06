using DojoManagerApi.Entities;
using System.Linq;
using System;
using System.ComponentModel;
using PropertyChanged;
using DojoManagerApi;
using Microsoft.Toolkit.Mvvm.Input;
using System.Timers;
using System.Windows.Threading;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DojoManagerGui.ViewModels
{
    public class EnumKendDegreeUtils
    {
        public static List<object> GetDegreeList()
        {
            List<object> list = new List<object>()
            {
                new { Value= KendoDegree.Kyu6, Description = "6° Kyu" },
                new { Value= KendoDegree.Kyu5, Description = "5° Kyu" },
                new { Value= KendoDegree.Kyu4, Description = "4° Kyu" },
                new { Value= KendoDegree.Kyu3, Description = "3° Kyu" },
                new { Value= KendoDegree.Kyu2, Description = "2° Kyu" },
                new { Value= KendoDegree.Kyu1, Description = "1° Kyu" },

                new { Value= KendoDegree.Dan1, Description = "1° Dan" },
                new { Value= KendoDegree.Dan2, Description = "2° Dan" },
                new { Value= KendoDegree.Dan3, Description = "3° Dan" },
                new { Value= KendoDegree.Dan4, Description = "4° Dan" },
                new { Value= KendoDegree.Dan5, Description = "5° Dan" },
                new { Value= KendoDegree.Dan6, Description = "6° Dan" },
                new { Value= KendoDegree.Dan7, Description = "7° Dan" },
                new { Value= KendoDegree.Dan8, Description = "8° Dan" },
            };
            return list;
        }
    }
    public class VM_Person : INotifyPropertyChanged
    {
        public VM_Person(Person person)
        {

            Person = (Person)EntityWrapper.Wrap(person);
            var hash = Person.GetHashCode();
            AddNewCard = new RelayCommand(
                () => Person.AddCard(
                           new MembershipCard()
                           {
                               ValidityStartDate = DateTime.Now,
                               ExpirationDate = DateTime.Now
                           }
                       ));
            RemoveCard = new RelayCommand<MembershipCard>(async c =>
            {
                if (c != null)
                    await App.AskAndExecuteAsync(() =>
                    {
                        Person.RemoveCard(c);
                    });
            });
            AddNewCertificate = new RelayCommand(() => Person.AddCertificate(new Certificate() { Expiry = DateTime.Now }));
            RemoveCertificate = new RelayCommand<Certificate>(async c =>
            {
                if (c != null)
                    await App.AskAndExecuteAsync(() =>
                    {
                        Person.RemoveCertificate(c);
                    });

            });
            ShowCertificateImageCommand = new RelayCommand<Certificate>(ShowCertificateImage);
            AddSubscriptionCommand = new RelayCommand(
                    () => Person.AddSubscription(new Subscription() { StartDate = DateTime.Now, EndDate = DateTime.Now }, 0),
                    () => Person != null);

            RemoveSubscriptionCommand = new RelayCommand<Subscription>(
                    async s =>
                    {
                        await App.AskAndExecuteAsync(() =>
                        {
                            Person.RemoveSubscription(s);
                            this.OnPropertyChanged(nameof(Payments));
                        });
                    }
                );
            AddPaymentCommand = new RelayCommand<Subscription>(
                    s =>
                    {
                        if (s != null)
                        {
                            var mov = s.Debit.AddPayment(0, DateTime.Now, Person.Origin);
                            App.Db.Save();
                            this.OnPropertyChanged(nameof(Payments));
                            WeakReferenceMessenger.Default.Send(
                                new EntityListChangedMessage<MoneyMovement>(
                                    this,
                                    new MoneyMovement[] { mov },
                                    Array.Empty<MoneyMovement>()));

                        }
                    }
                ); ;
            RemovePaymentCommand = new RelayCommand<DebitPayment>(async dp =>
            {
                if (dp != null)
                    await App.AskAndExecuteAsync(() =>
                    {
                        dp.Debit.RemovePayment(dp);
                        App.Db.Delete(dp);
                        App.Db.Save();
                        this.OnPropertyChanged(nameof(Payments));
                        WeakReferenceMessenger.Default.Send(
                            new EntityListChangedMessage<MoneyMovement>(
                                this,
                                Array.Empty<MoneyMovement>(),
                                new MoneyMovement[] { dp }));
                    });

            });

            AddExaminationCommand = new RelayCommand(() => Person.AddNewExamination());
            RemoveExaminationCommand = new RelayCommand<KendoExamination>(async e =>
            {
                await App.AskAndExecuteAsync(() =>
                {
                    if (e != null)
                        Person.RemoveExamination(e);
                });
            });

            SetPersonPictureCommand = new RelayCommand(() => SetPersonPicture());

            IsMember = Person.Cards.Where(c =>
                c.Association == Config.Instance.NomeAssociazione
                && !c.Invalidated
                && DateTime.Now < c.ExpirationDate && DateTime.Now >= c.ValidityStartDate).Any();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();

            dispatcherTimer.Interval = TimeSpan.FromSeconds(0.3);
            dispatcherTimer.Tick += new EventHandler((s, e) =>
            {
                this.OnPropertyChanged(nameof(Debit));
                //this.OnPropertyChanged(nameof(Payments));
            });
            dispatcherTimer.Start();
        }


        public bool IsMember { get; private set; }
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Person Person { get; }
        public decimal Debit => Person.Subscriptions.Select(s => s.Debit).Sum(d => d.Amount - d.Payments.Sum(pay => pay.Amount));
        public DateTime? CertiFicateExpiration => Person.Certificates.OrderByDescending(c => c.Expiry).FirstOrDefault()?.Expiry;
        public DateTime? DojoSubscription => Person.Cards
            .Where(s => s.Association.Equals(App.ClubName, StringComparison.InvariantCultureIgnoreCase))
            .OrderByDescending(c => c.ValidityStartDate)
            .FirstOrDefault()
            ?.ValidityStartDate;

        public IEnumerable<DebitPayment> Payments => Person.Subscriptions.SelectMany(s => s.Debit.Payments).ToList();
        public IList<Subscription> Subscriptions => Person.Subscriptions;
        public string[] DefaultSubscriptions { get; } = new string[] { "Iscrizione annuale Kensei Dojo", "Iscrizione annuale CIK" };

        public RelayCommand AddSubscriptionCommand { get; }
        public RelayCommand<Subscription> RemoveSubscriptionCommand { get; }
        public RelayCommand<Subscription> AddPaymentCommand { get; }

        public RelayCommand AddNewCard { get; }
        public RelayCommand<MembershipCard> RemoveCard { get; }
        public RelayCommand AddNewCertificate { get; }
        public RelayCommand<Certificate> RemoveCertificate { get; }
        public RelayCommand<Certificate> ShowCertificateImageCommand { get; }
        public RelayCommand<DebitPayment> RemovePaymentCommand { get; }
        public RelayCommand AddExaminationCommand { get; }
        public RelayCommand<KendoExamination> RemoveExaminationCommand { get; }
        public RelayCommand SetPersonPictureCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;


        public void ShowCertificateImage(Certificate cert)
        {
            var vm = new VM_ImageViewer(cert);
            var win = new Window_ImageViewer() { DataContext = vm };
            win.ShowDialog();
            cert.ImageFileName = vm.ImageFilePath;
        }

        public void SetPersonPicture()
        {
            var selectedPicture = App.SelectImage();
            if (selectedPicture != null)
            {
                App.Db.SetImage(Person, selectedPicture);
            }
            OnPropertyChanged(nameof(PersonPicture));
        }
        public List<object> KendoDegrees => EnumKendDegreeUtils.GetDegreeList();
        public string[] SubscriptionsSuggested => Config.Instance.SuggerimentiSottoscrizioni;
        public string[] CardsSuggested => Config.Instance.SuggerimentiAssociazioni;
        public ImageSource PersonPicture
        {
            get
            {
                var bytes = App.Db.GetImageBytes(Person);
                BitmapImage image = null;
                if (bytes.Length > 0)
                {
                    image = new BitmapImage();
                    using (MemoryStream mem = new MemoryStream(bytes))
                    {
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = mem;
                        image.EndInit();
                        image.Freeze();
                    }
                }
                return image;
            }
        }
    }
}
