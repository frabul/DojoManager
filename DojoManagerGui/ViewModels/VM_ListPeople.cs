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
    public class VM_ListPeople : VM_FunctionPage, INotifyPropertyChanged
    {
        private int? LastPersonSelectedId;
        private bool showMembers = true;
        private bool showNonMembers = true;

        public override string Name => "Persone";
        public ObservableCollection<VM_Person> People { get; set; }
        public VM_Person? PersonSelected { get; set; }
        public RelayCommand AddNewPersonCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand<VM_Person> RemovePersonCommand { get; }
        public string NameFilterString { get; set; }

        public VM_ListPeople()
        {
            RefreshPeople();
            PersonSelected = People?.FirstOrDefault();
            AddNewPersonCommand = new RelayCommand(AddNewPerson);
            SearchCommand = new RelayCommand(SearchPerson);
            RemovePersonCommand = new RelayCommand<VM_Person>(RemovePerson);
            WeakReferenceMessenger.Default.Register<EntityListChangedMessage<Subject>>(this,
                (r, a) =>
                {
                    VM_ListPeople rec = (VM_ListPeople)r;
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
                //check that person is not associated with any debit
                var movements = App.Db.ListMovements(DateTime.MinValue, DateTime.MaxValue, vm.Person.Id);
                if (movements.Any())
                    await App.ShowMessage("Errore", "Non è possibile rimuovere questa persona perchè è associata a delle movimenti finanziari.");
                else
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
        public bool ShowMembers { get => showMembers; set { showMembers = value; RefreshPeople(); } }
        public bool ShowNonMembers { get => showNonMembers; set { showNonMembers = value; RefreshPeople(); } }
        private void RefreshPeople()
        {
            PushPersonSelected();

            IEnumerable<Person> ppl = App.Db.ListPeople();
            if (!string.IsNullOrWhiteSpace(NameFilterString))
                ppl = ppl.Where(p => p.Name.Contains(NameFilterString, StringComparison.InvariantCultureIgnoreCase));
            var p2 = ppl.Select(p => new VM_Person(p)).Where(p => (p.IsMember && ShowMembers) || (!p.IsMember && ShowNonMembers));

            People = new ObservableCollection<VM_Person>(p2);
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
