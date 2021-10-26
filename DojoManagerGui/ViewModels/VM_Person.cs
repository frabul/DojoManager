using DojoManagerApi.Entities;
using System.Linq;
using System;
using System.ComponentModel;
using PropertyChanged;
using DojoManagerApi;
using Microsoft.Toolkit.Mvvm.Input;
using System.Timers;
using System.Windows.Threading;

namespace DojoManagerGui.ViewModels
{ 
    public class VM_Person : INotifyPropertyChanged
    {
        public VM_Person(Person person)
        {
            Person = (Person)EntityWrapper.Wrap(person);
            var hash = Person.GetHashCode();
            Subscriptions = new VM_PersonSubscriptions(Person);
            AddNewCard = new RelayCommand(() => Person.AddCard(new Card()));
            RemoveCard = new RelayCommand<Card>(c => Person.RemoveCard(c));
            AddNewCertificate = new RelayCommand(() => Person.AddCertificate(new Certificate()));
            RemoveCertificate = new RelayCommand<Certificate>(c => Person.RemoveCertificate(c));
            ShowCertificateImage = new RelayCommand<Certificate>(ShowImage);
            DispatcherTimer dispatcherTimer = new  DispatcherTimer();
          
            dispatcherTimer.Interval = TimeSpan.FromSeconds(0.3);
            dispatcherTimer.Tick += new EventHandler((s,e) => this.OnPropertyChanged(nameof(Debit)));
            dispatcherTimer.Start();

        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Person Person { get; }
        public decimal Debit => Person.Subscriptions.Select(s => s.Debit).Sum(d => d.Amount - d.Payments.Sum(pay => pay.Amount));
        public DateTime? CertiFicateExpiration => Person.Certificates.OrderByDescending(c => c.Expiry).FirstOrDefault()?.Expiry;
        public DateTime? DojoSubscription => Person.Subscriptions.Where(s => s.Type == SubscriptionType.Kensei_Dojo_Annual_Association).OrderByDescending(c => c.StartDate).FirstOrDefault()?.StartDate;


        public VM_PersonSubscriptions Subscriptions { get; }

        public RelayCommand AddNewCard { get; }
        public RelayCommand<Card> RemoveCard { get; }
        public RelayCommand AddNewCertificate { get; }
        public RelayCommand<Certificate> RemoveCertificate { get; }
        public RelayCommand<Certificate> ShowCertificateImage { get; }

        public event PropertyChangedEventHandler? PropertyChanged;


        public void ShowImage(Certificate cert)
        {
            var vm = new VM_ImageViewer(cert.ImagePath);
            var win  = new  Window_ImageViewer() {  DataContext = vm };
            win.ShowDialog();
            cert.ImagePath = vm.ImageFilePath;
        }
    }
}
