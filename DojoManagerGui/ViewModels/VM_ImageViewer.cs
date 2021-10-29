using DojoManagerApi.Entities;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerGui.ViewModels
{
    public class VM_ImageViewer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? ImageFilePath { get; set; }
        public RelayCommand SelectImageCommand { get; }
        public string ImagesDirectory { get; }
        public Certificate Certificate { get; } 
        public VM_ImageViewer(Certificate certificate )
        {
            ImageFilePath = App.Db.GetImagePath(certificate); 
            SelectImageCommand = new RelayCommand(SelectImage);
        }


        public void SelectImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.bmp) | *.png;*.jpeg;*.bmp";
            openFileDialog.InitialDirectory = ImagesDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFile = openFileDialog.FileName; 
                App.Db.SetImage(Certificate, selectedFile); 
            }
        }
    }
}
