using DojoManagerApi;
using DojoManagerApi.Entities;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerGui.ViewModels
{
    public class VM_Subjects : VM_FunctionPage
    {
        private Subject? _SubjectSelected;
        private int? LastSubjectSelectedId;

        public override string Name => "Soggetti";
        public override string IconPath => "M21,5C19.89,4.65 18.67,4.5 17.5,4.5C15.55,4.5 13.45,4.9 12,6C10.55,4.9 8.45,4.5 6.5,4.5C4.55,4.5 2.45,4.9 1,6V20.65C1,20.9 1.25,21.15 1.5,21.15C1.6,21.15 1.65,21.1 1.75,21.1C3.1,20.45 5.05,20 6.5,20C8.45,20 10.55,20.4 12,21.5C13.35,20.65 15.8,20 17.5,20C19.15,20 20.85,20.3 22.25,21.05C22.35,21.1 22.4,21.1 22.5,21.1C22.75,21.1 23,20.85 23,20.6V6C22.4,5.55 21.75,5.25 21,5M21,18.5C19.9,18.15 18.7,18 17.5,18C15.8,18 13.35,18.65 12,19.5V8C13.35,7.15 15.8,6.5 17.5,6.5C18.7,6.5 19.9,6.65 21,7V18.5Z";

        public string? NameFilterString { get; set; }
        public RelayCommand SearchCommand { get; }
        public ObservableCollection<Subject> Subjects { get; set; }
        public Subject? SubjectSelected
        {
            get => _SubjectSelected; set
            {

                _SubjectSelected = value;
                if (value != null)
                    MovementsOfSubjectSelected = new VM_MoneyMovements(value);
                else
                    MovementsOfSubjectSelected = null;
            }
        }



        public RelayCommand AddNewSubjectCommand { get; }
        public RelayCommand<Subject> RemoveSubjectCommand { get; }
        public VM_MoneyMovements? MovementsOfSubjectSelected { get; set; }
        public VM_Subjects()
        {
            Subjects = new ObservableCollection<Subject>();
            RefreshSubjects();
            SubjectSelected = Subjects.First();

            AddNewSubjectCommand = new RelayCommand(() =>
            {
                var sb = App.Db.AddNewSubject("Vecchia Fattoria");
                this.Subjects.Add(sb);
                App.Db.Save();
                WeakReferenceMessenger.Default.Send(
                    new EntityListChangedMessage<Subject>(this, new Subject[] { sb }, Array.Empty<Subject>()));
            });

            RemoveSubjectCommand = new RelayCommand<Subject>(async sb =>
            {
                if (sb != null) 
                    await App.AskAndExecuteAsync(() => RemoveSubject(sb)); 
            });
            SearchCommand = new RelayCommand(() => RefreshSubjects());
            WeakReferenceMessenger.Default.Register<EntityListChangedMessage<Subject>>(
                this, (r, a) =>
                {
                    var rec = (VM_Subjects)r;
                    if(rec != a.Sender) 
                        rec.RefreshSubjects(); 
                });
        }

        private void RemoveSubject(Subject sb)
        {
            PushSelected();
            //verifichiamo che non abbiamo transazioni associate
            var movementsOfsb = App.Db.ListEntities<MoneyMovement>(q => q.Where(mov => mov.Counterpart.Id == sb.Id));
            if (movementsOfsb.Count < 1)
            {
                App.Db.Delete(sb);
                this.Subjects.Remove(sb);
                App.Db.Save();
                WeakReferenceMessenger.Default.Send<EntityListChangedMessage<Subject>>(
                       new EntityListChangedMessage<Subject>(this, new Subject[0] { }, new[] { sb }));
            }
            else
            {
                App.ShowMessage("Attenzione!", $"Non è possibile cancellare il soggetto perchè ha {movementsOfsb.Count} movimenti di denaro associati.");
            }
            PopSelected();
        }

        private void RefreshSubjects()
        {
            PushSelected();
            IEnumerable<Subject> subjects = App.Db.ListSubjects();
            if (!string.IsNullOrWhiteSpace(NameFilterString))
                subjects = subjects.Where(s => s.Name.Contains(NameFilterString, StringComparison.InvariantCultureIgnoreCase));
            subjects = subjects.Select(s => (Subject)EntityWrapper.Wrap(s));
            Subjects = new ObservableCollection<Subject>(subjects);
            PopSelected();
        }

        private void PopSelected()
        {
            SubjectSelected = Subjects.FirstOrDefault(p => p.Id == LastSubjectSelectedId);
        }

        private void PushSelected()
        {
            LastSubjectSelectedId = SubjectSelected?.Id;
        }
    }
}
