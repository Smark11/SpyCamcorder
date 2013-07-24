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
            try
            {
                InitializeComponent();
                Files = new ObservableCollection<Video>();
                DataContext = this;
                LoadFiles();
            }
            catch (Exception ex)
            {

            }
        }

        private void LoadFiles()
        {
            try
            {
                string[] files;

                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    files = iso.GetFileNames();
                }

                foreach (var row in files)
                {
                    if (row.ToUpper().Contains("BACK") || row.ToUpper().Contains("FRONT"))
                    {
                        Files.Add(new Video(row));
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private ObservableCollection<Video> _files;
        public ObservableCollection<Video> Files
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

        private void PlayClicked(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Video video = btn.DataContext as Video;

            try
            {
                NavigationService.Navigate(new Uri("/VideoReview.xaml?parameter=" + video.FileName, UriKind.Relative));
            }
            catch (Exception ex)
            {

            }
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Video video = btn.DataContext as Video;
            
            try
            {
                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    iso.DeleteFile(video.FileName);

                    Files.Remove(video);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public class Video : INotifyPropertyChanged
    {
        public Video(string fileName)
        {
            FileName = fileName;
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        

        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}