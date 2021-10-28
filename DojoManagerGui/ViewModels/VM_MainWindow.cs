using DojoManagerApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerGui.ViewModels
{
    public class VM_MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<VM_FunctionPage> FunctionPages { get; set; } = new ObservableCollection<VM_FunctionPage>();

        public VM_FunctionPage FunctionSelected { get; set; }

        public VM_MainWindow()
        {
            FunctionPages = new ObservableCollection<VM_FunctionPage>()
            {
                new VM_ListPersons(),
                new VM_DummyPage("page 2"),
                new VM_MoneyMovements()
            };
            FunctionSelected = FunctionPages[0];
        }
    }

    public class VM_DummyPage : VM_FunctionPage
    {
        public override string Name { get; }
        public VM_DummyPage(string name)
        {
            Name = name;
        }
    }

    public abstract class VM_FunctionPage : INotifyPropertyChanged
    {
        public abstract string Name { get; }
        public virtual string IconPath { get; } = "M16 17V19H2V17S2 13 9 13 16 17 16 17M12.5 7.5A3.5 3.5 0 1 0 9 11A3.5 3.5 0 0 0 12.5 7.5M15.94 13A5.32 5.32 0 0 1 18 17V19H22V17S22 13.37 15.94 13M15 4A3.39 3.39 0 0 0 13.07 4.59A5 5 0 0 1 13.07 10.41A3.39 3.39 0 0 0 15 11A3.5 3.5 0 0 0 15 4Z";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
