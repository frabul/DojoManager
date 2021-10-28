using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DojoManagerApi.Entities;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.Input;
using DojoManagerApi;

namespace DojoManagerGui.ViewModels
{
    public class VM_PersonSubscriptions : INotifyPropertyChanged
    {
        public Person Person { get; set; }

        public RelayCommand AddSubscriptionCommand { get; }
        public RelayCommand<Subscription> RemoveSubscriptionCommand { get; }
        public RelayCommand<Subscription> AddPaymentCommand { get; }

        public IList<Subscription> Subscriptions => Person.Subscriptions;
        public string[] DefaultSubscriptions { get; } = new string[] { "Iscrizione annuale Kensei Dojo", "Iscrizione annuale CIK" };
        public event PropertyChangedEventHandler? PropertyChanged;

        public VM_PersonSubscriptions(Person person)
        {

            Person = (Person)EntityWrapper.Wrap(person);
            AddSubscriptionCommand = new RelayCommand(
                    () => Person.AddSubscription(new Subscription(), 0),
                    () => Person != null);

            RemoveSubscriptionCommand = new RelayCommand<Subscription>(
                    s => Person.RemoveSubscription(s)
                );
            AddPaymentCommand = new RelayCommand<Subscription>(
                    s => s.Debit.AddPayment(0, DateTime.Now, Person.Origin)
                );
        }

    }
}
