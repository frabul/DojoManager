using DojoManagerApi.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System;
namespace DojoManagerGui.ViewModels
{
    public class VM_ListAssociates : VM_FunctionPage
    {
        public override string Name => "Lista soci";
        public ObservableCollection<MemberViewForDatagrid> Members { get; set; }  

        public VM_ListAssociates( )
        { 
            var pvms = App.Db.ListPersons()
                .Select(p => p)
                .Select(p => new MemberViewForDatagrid(p));
            Members = new ObservableCollection<MemberViewForDatagrid>( pvms );
        }


    }

    public class MemberViewForDatagrid
    {
        public MemberViewForDatagrid( Person person)
        {
            Person = person;
        }

        public Person Person { get; }
        public string Name => Person.Name;
        public DateTime BirthDate => Person.BirthDate;
        public decimal Debit => Person.Subscriptions.Select(s=>s.Debit).Sum(d => d.Amount - d.Payments.Sum(pay => pay.Amount));
        public DateTime? CertiFicateExpiration => Person.Certificates.OrderByDescending(c => c.Expiry).FirstOrDefault()?.Expiry;
        public DateTime? DojoSubscription => Person.Subscriptions.Where(s => s.Type == SubscriptionType.Kensei_Dojo_Annual_Association).OrderByDescending(c => c.StartDate).FirstOrDefault()?.StartDate;
    }
}
