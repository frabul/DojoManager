using DojoManagerApi.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace DojoManagerGui.ViewModels
{
    public class VM_ListPersons : VM_FunctionPage, INotifyPropertyChanged
    {
        public override string Name => "Persone";
        public ObservableCollection<VM_Person> Persons { get; set; }
        public VM_Person? PersonSelected { get; set; }
        public RelayCommand AddNewPersonCommand { get; }
        public RelayCommand<string> SearchCommand { get; }
        public RelayCommand<VM_Person> RemovePersonCommand { get; }

        public VM_ListPersons()
        {
            RefreshPersons();
            PersonSelected = Persons?.FirstOrDefault();
            AddNewPersonCommand = new RelayCommand(AddNewPerson);
            SearchCommand = new RelayCommand<string>(SearchPerson);
            RemovePersonCommand = new RelayCommand<VM_Person>(RemovePerson);
        }

        public void SearchPerson(string? nameFilter)
        {
            if(nameFilter != null)
            {
                int pidSelected = -1;
                if (PersonSelected != null)
                    pidSelected = PersonSelected.Person.Id;
                var pvms = App.Db.ListPersons()
                    .Where( p => p.Name?.Contains(nameFilter) == true)
                    .Select(p => new VM_Person(p));
                Persons = new ObservableCollection<VM_Person>(pvms);
                PersonSelected = Persons.FirstOrDefault(p => p.Person.Id == pidSelected);
            }
        }
        public void RemovePerson(VM_Person? vm)
        {
            if(vm != null)
            {
                int pidSelected = -1;
                if (PersonSelected != null)
                    pidSelected = PersonSelected.Person.Id;
                App.Db.RemovePerson(vm.Person.Origin);
                RefreshPersons(); 
                PersonSelected = Persons.FirstOrDefault(p => p.Person.Id == pidSelected);
            } 
        }

        public void AddNewPerson()
        {
            var person = App.Db.AddNewPerson();
            int pidSelected = -1;
            if (PersonSelected != null)
                pidSelected = PersonSelected.Person.Id;
            RefreshPersons(); 
            PersonSelected = Persons.FirstOrDefault(p=> p.Person.Id == pidSelected);

        }
        private void RefreshPersons()
        {
            var pvms = App.Db.ListPersons()
                            .Select(p => p)
                            .Select(p => new VM_Person(p));
            Persons = new ObservableCollection<VM_Person>(pvms);
        }


    }
}
