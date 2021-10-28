using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DojoManagerApi;
using DojoManagerApi.Entities;
using Microsoft.Toolkit.Mvvm.Input;

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
        public List<VM_MoneyMovement> Movements { get; set; }
        public override string Name => "Entrate/Uscite";
        public override string IconPath => "M600 100C323.85 100 100 323.85 100 600S323.85 1100 600 1100S1100 876.15 1100 600S876.15 100 600 100zM600 200A400 400 0 1 1 600 1000A400 400 0 0 1 600 200zM502.5000000000001 650H750V550H502.5000000000001A125 125 0 0 1 705.7 479.5L790.7 422.85A225 225 0 0 0 401.4000000000001 550H350V650H401.35A225 225 0 0 0 790.75 777.15L705.7 720.5A125 125 0 0 1 502.5000000000001 650z";

        public RelayCommand<VM_MoneyMovement> RemoveMovement { get; }

        public VM_MoneyMovements()
        {
            Movements = new List<VM_MoneyMovement>();
            UpdateMovements();
            RemoveMovement = new RelayCommand<VM_MoneyMovement>(
                m =>
                {
                }
            );
        }

        public void UpdateMovements()
        {
            var movements = App.Db.GetMoneyMovements();
            Movements = new List<VM_MoneyMovement>(movements.Select(m => new VM_MoneyMovement(m)));
        }


    }
}
