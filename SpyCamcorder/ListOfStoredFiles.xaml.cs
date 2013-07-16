using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SpyCamcorder
{
    public partial class ListOfStoredFiles : PhoneApplicationPage, INotifyPropertyChanged
    {
        public ListOfStoredFiles()
        {
            InitializeComponent();
            Files = new ObservableCollection<string>();
            DataContext = this;
            LoadFiles();
        }

        private void LoadFiles()
        {
            string[] files;

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                files = iso.GetFileNames();
            }

            foreach (var row in files)
            {
                if (row.ToUpper().Contains("SPY"))
                {
                    Files.Add(row);
                }
            }
        }

        private ObservableCollection<string> _files;
        public ObservableCollection<string> Files
        {
            get { return _files; }
            set { _files = value; RaisePropertyChanged("Files"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private void FileClicked(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                NavigationService.Navigate(new Uri("/VideoReview.xaml?parameter=" + btn.Content, UriKind.Relative));
            }
            catch (Exception ex)
            {

            }
        }
    }
}