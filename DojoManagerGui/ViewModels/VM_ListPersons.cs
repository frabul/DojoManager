using DojoManagerApi.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System;

namespace DojoManagerGui.ViewModels
{
    public class VM_ListPersons : VM_FunctionPage, INotifyPropertyChanged
    {
        private int? LastPersonSelectedId;

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
            WeakReferenceMessenger.Default.Register<EntityListChangedMessage<Subject>>(this,
                (r, a) =>
                {
                    VM_ListPersons rec = (VM_ListPersons)r;
                    if (rec != a.Sender)
                        rec.RefreshPeople();
                });
        }

        public void SearchPerson()
        {
            RefreshPeople();
        }
        public async void RemovePerson(VM_Person? vm)
        {
            if (vm != null)
            {
                await App.AskAndExecuteAsync(() =>
                {
                    PushPersonSelected();
                    App.Db.Delete(vm.Person);
                    App.Db.Save();
                    People.Remove(vm);
                    WeakReferenceMessenger.Default.Send<EntityListChangedMessage<Subject>>(
                        new EntityListChangedMessage<Subject>(this, new Subject[0] { }, new[] { vm.Person }));
                    PopPersonSelected();
                });
            }
        }

        public void AddNewPerson()
        {

            PushPersonSelected();
            var person = App.Db.AddNewPerson("John Doe");
            People.Add(new VM_Person(person));
            App.Db.Save();
            WeakReferenceMessenger.Default.Send<EntityListChangedMessage<Subject>>(
                        new EntityListChangedMessage<Subject>(this, new Subject[] { person }, new Subject[] { }));
            PopPersonSelected();
        }

        private void RefreshPeople()
        {
            PushPersonSelected();

            IEnumerable<Person> ppl = App.Db.ListPeople();
            if (!string.IsNullOrWhiteSpace(NameFilterString))
                ppl = ppl.Where(p => p.Name.Contains(NameFilterString, StringComparison.InvariantCultureIgnoreCase));
            People = new ObservableCollection<VM_Person>(ppl.Select(p => new VM_Person(p)));

            PopPersonSelected();

        }

        private void PopPersonSelected()
        {
            PersonSelected = People.FirstOrDefault(p => p.Person.Id == LastPersonSelectedId);
        }

        private void PushPersonSelected()
        {
            LastPersonSelectedId = PersonSelected?.Person.Id;
        }
    }
}
