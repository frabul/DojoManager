using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DojoManagerApi;
using DojoManagerApi.Entities;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DojoManagerGui.ViewModels
{
    public class VM_MoneyMovement
    {
        public MoneyMovement Movement { get; set; }
        public DateTime Date { get; set; }
        public VM_MoneyMovement(MoneyMovement movement)
        {
            Movement = (MoneyMovement)EntityWrapper.Wrap(movement);
        }
    }
    public class VM_MoneyMovements : VM_FunctionPage, INotifyPropertyChanged
    {
        public Subject SubjectAssociated { get; }
        public ObservableCollection<MoneyMovement> Movements { get; set; }
        public override string Name => "Entrate/Uscite";
        public override string IconPath => "M600 100C323.85 100 100 323.85 100 600S323.85 1100 600 1100S1100 876.15 1100 600S876.15 100 600 100zM600 200A400 400 0 1 1 600 1000A400 400 0 0 1 600 200zM502.5000000000001 650H750V550H502.5000000000001A125 125 0 0 1 705.7 479.5L790.7 422.85A225 225 0 0 0 401.4000000000001 550H350V650H401.35A225 225 0 0 0 790.75 777.15L705.7 720.5A125 125 0 0 1 502.5000000000001 650z";

        public RelayCommand<MoneyMovement> RemoveMovementCommand { get; }
        public RelayCommand AddNewMovementCommand { get; }

        public VM_MoneyMovements(Subject subjectAssociated)
        {
            SubjectAssociated = subjectAssociated;
            Movements = new ObservableCollection<MoneyMovement>();
            Refresh();
            RemoveMovementCommand = new RelayCommand<MoneyMovement>(RemoveMovementF);
            AddNewMovementCommand = new RelayCommand(AddNewMovement);
            WeakReferenceMessenger.Default.Register<EntityListChangedMessage<MoneyMovement>>(this,
                (r, a) =>
                {
                    var rec = (VM_MoneyMovements)r;
                    if(rec != a.Sender) 
                        this.Refresh(); 
                });
        }



        public void Refresh()
        {
            IEnumerable<MoneyMovement> movements = Enumerable.Empty<MoneyMovement>();
            if (SubjectAssociated != null)
            {
                movements = App.Db.ListMovements(DateTime.MinValue, DateTime.MaxValue, SubjectAssociated.Id);
            }
            else
            {
                movements = App.Db.ListMovements(DateTime.MinValue, DateTime.MaxValue);

            }
            Movements = new ObservableCollection<MoneyMovement>(
                    movements.Select(m => (MoneyMovement)EntityWrapper.Wrap(m)));
        }

        public void AddNewMovement()
        {
            if (SubjectAssociated != null)
            {
                var mov = App.Db.AddNewMovement(SubjectAssociated);
                this.Movements.Add(mov);
                App.Db.Save();
                WeakReferenceMessenger.Default.Send(
                   new EntityListChangedMessage<MoneyMovement>(this, new MoneyMovement[] { mov }, Array.Empty<MoneyMovement>()));
            }
        }
        private void RemoveMovementF(MoneyMovement? obj)
        {
            if (obj != null)
            {
                if (obj is DebitPayment p)
                {
                    App.ShowMessage("Attenzione!",
                        "Non è possibile rimuovere questo movimento da questa schermata poichè ha un debito associato." + Environment.NewLine +
                        $"Prova a rimuoverlo dalla lista pagamenti della persona ({p.Notes}).");
                }
                else
                {
                    App.Db.Delete(obj);
                    App.Db.Save();
                    this.Movements.Remove(obj);
                    WeakReferenceMessenger.Default.Send(
                        new EntityListChangedMessage<MoneyMovement>(this, Array.Empty<MoneyMovement>(), new MoneyMovement[] { obj }));
                }
            }
        }

    }
}
