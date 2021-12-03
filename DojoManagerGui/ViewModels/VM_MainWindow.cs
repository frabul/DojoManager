using DojoManagerApi;
using Microsoft.Toolkit.Mvvm.Input;
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
        public RelayCommand BackupCommand { get; }
        public VM_MainWindow()
        {
            FunctionPages = new ObservableCollection<VM_FunctionPage>()
            {
                new VM_ListPeople(),
                new VM_Subjects(),
                new VM_MoneyMovements(null)
            };
            FunctionSelected = FunctionPages[0];
            BackupCommand = new RelayCommand(BackupAsync);
        }
        public async void BackupAsync()
        {
            var fileName = await App.SelectBackupFile();
            if(fileName != null) 
                App.Db.CreateBackUp(fileName);
        }
    }
}
