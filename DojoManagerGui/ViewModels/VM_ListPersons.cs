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
        public ObservableCollection<VM_Person> People { get; set; }
        public VM_Person? PersonSelected { get; set; }
        public RelayCommand AddNewPersonCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand<VM_Person> RemovePersonCommand { get; }
        public string NameFilterString { get; set; }
        public VM_ListPersons()
        {
            RefreshPeople();
            PersonSelected = People?.FirstOrDefault();
            AddNewPersonCommand = new RelayCommand(AddNewPerson);
            SearchCommand = new RelayCommand(SearchPerson);
            RemovePersonCommand = new RelayCommand<VM_Person>(RemovePerson);
        }

        public void SearchPerson()
        {
            if (NameFilterString != null)
            {
                int pidSelected = -1;
                if (PersonSelected != null)
                    pidSelected = PersonSelected.Person.Id;
                var pvms = App.Db.ListPeople()
                    .Where(p => p.Name?.Contains(NameFilterString, System.StringComparison.InvariantCultureIgnoreCase) == true)
                    .Select(p => new VM_Person(p));
                People = new ObservableCollection<VM_Person>(pvms);
                PersonSelected = People.FirstOrDefault(p => p.Person.Id == pidSelected);
            }
        }
        public void RemovePerson(VM_Person? vm)
        {
            if (vm != null)
            {
                int pidSelected = -1;
                if (PersonSelected != null)
                    pidSelected = PersonSelected.Person.Id;
                App.Db.Delete(vm.Person);
                People.Remove(vm);

                PersonSelected = People.FirstOrDefault(p => p.Person.Id == pidSelected);
            }
        }

        public void AddNewPerson()
        {
            var person = App.Db.AddNewPerson("John Doe");
            int pidSelected = -1;
            if (PersonSelected != null)
                pidSelected = PersonSelected.Person.Id;
            People.Add(new VM_Person(person));
            PersonSelected = People.FirstOrDefault(p => p.Person.Id == pidSelected);
        }

        private void RefreshPeople()
        {
            var pvms = App.Db.ListPeople()
                            .Select(p => p)
                            .Select(p => new VM_Person(p));
            People = new ObservableCollection<VM_Person>(pvms);
        }


    }
}
