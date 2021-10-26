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
        public VM_ImageViewer(string imageName)
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ImagesDirectory = Path.Combine(assemblyDir, "CertificatesImages");
            if (imageName != null)
            {
                ImageFilePath = Path.Combine(ImagesDirectory, imageName);
            }
          
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
                var selectedFileName = Path.GetFileName(selectedFile);
                var selectedFileDir = Path.GetDirectoryName(selectedFile);
                if (selectedFileDir != ImagesDirectory)
                {
                    Directory.CreateDirectory(ImagesDirectory);
                    File.Copy(selectedFile, Path.Combine(ImagesDirectory, selectedFileName));
                }
                this.ImageFilePath = Path.Combine(ImagesDirectory, selectedFileName);
            }
        }
    }
}
