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
                new VM_DummyPage("page 3")
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

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
