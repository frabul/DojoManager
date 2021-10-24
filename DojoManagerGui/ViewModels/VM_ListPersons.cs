using DojoManagerApi.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;

namespace DojoManagerGui.ViewModels
{
    public class VM_ListPersons : VM_FunctionPage, INotifyPropertyChanged
    {
        public override string Name => "Lista soci";
        public ObservableCollection<VM_Person> Persons { get; set; }
        public VM_Person PersonSelected { get; set; }

   
        public VM_ListPersons()
        {
            var pvms = App.Db.ListPersons()
                .Select(p => p)
                .Select(p => new VM_Person(p));
            Persons = new ObservableCollection<VM_Person>(pvms);
        }


    }
}
