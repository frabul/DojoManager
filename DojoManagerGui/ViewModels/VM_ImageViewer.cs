using DojoManagerApi.Entities;
using Microsoft.AspNetCore.Components;
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
        public Certificate Certificate { get; }
        public VM_ImageViewer(Certificate certificate)
        {
            Certificate = certificate;
            ImageFilePath = App.Db.GetImagePath(certificate);
            SelectImageCommand = new RelayCommand(SelectImage);
        }


        public void SelectImage()
        {
            var selectedFile = App.SelectImage();
            if (selectedFile != null)
            {
                ImageFilePath = null;
                
                App.Current.Dispatcher.InvokeAsync(( ) =>
                {
                    try 
                    { 
                        App.Db.SetImage(Certificate, selectedFile);
                        ImageFilePath = App.Db.GetImagePath(Certificate);
                    }
                    catch(Exception ex) { } 
                });
               
            }


        }


    }
}
