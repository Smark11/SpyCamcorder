using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SpyCamcorder.Resources;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Live;
using Microsoft.Phone.BackgroundTransfer;
using Common.IsolatedStoreage;
using Microsoft.Phone.Tasks;
using Common.Licencing;

namespace SpyCamcorder
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static MainPage _mainPageInstance;
        private const string VIDEONUMBER = "videonumber";

        CaptureSource _captureSource;
        VideoCaptureDevice _videoCaptureDevice;

        IsolatedStorageFileStream _isoVideoFile;
        FileSink _fileSink;
        string _isoVideoFileName = "CameraMovie.mp4";
        VideoBrush _videoRecorderBrush;

        private enum ButtonState { Initialized, Ready, Recording, Playback, Paused, NoChange, CameraNotSupported };
        ButtonState _currentAppState;

        // Constructor
        public MainPage()
        {
            try
            {
                _mainPageInstance = this;

                InitializeComponent();
                AppSettings.Initialize();
                WebBrowser.Source = new Uri("http://www.espn.com/mobile", UriKind.Absolute);

                // Sample code to localize the ApplicationBar
                //BuildLocalizedApplicationBar();
                InitalizeVideoRecorder();

                IsAppTrialOrBought();
            }
            catch (Exception ex)
            {

            }
        }

        public void IsAppTrialOrBought()
        {
            try
            {
                if ((Application.Current as App).IsTrial)
                {
                    Trial.SaveStartDateOfTrial();

                    if (Trial.IsTrialExpired())
                    {
                        MessageBox.Show("Your trial has expired.  Please support the developers, and purchase this application!");
                        RecordButton.IsEnabled = false;
                        MarketplaceDetailTask task = new MarketplaceDetailTask();
                        task.Show();
                        InitializeAppForTrialOrActivated(false);
                    }
                    else
                    {
                        if (AppSettings.IsSecondTimeOpen && !AppSettings.IsAppRated)
                        {
                            MessageBoxResult msgResult;
                            msgResult = MessageBox.Show("You have " + Trial.GetDaysLeftInTrial() + " days remaining in your trial.  To Extend your trial from 3 days to 10 Days, Rate this app 5 stars, and leave a positive comment. ", "Extend Trial?", MessageBoxButton.OKCancel);
                            if (msgResult == MessageBoxResult.OK)
                            {
                                Trial.Add10DaysToTrial();
                                AppSettings.SetAppAsRated();
                                MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                                marketplaceReviewTask.Show();
                            }
                        }
                        else
                        {
                            MessageBox.Show("You have " + Trial.GetDaysLeftInTrial() + " days remaining in your trial.");
                        }
                        InitializeAppForTrialOrActivated(true);
                    }
                }
                else
                {
                    InitializeAppForTrialOrActivated(true);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void InitializeAppForTrialOrActivated(bool unlocked)
        {
            try
            {
                RecordButton.IsEnabled = unlocked;
                //TODO Initialize app bar
                //ShowFilesButton.IsEnabled = unlocked;
                //StopRecordButton.IsEnabled = unlocked;
                //SettingsButton.IsEnabled = unlocked;
                //ReviewButton.IsEnabled = unlocked;
                //MoreAppsButton.IsEnabled = unlocked;
            }
            catch (Exception ex)
            {

            }
        }

        private void InitalizeVideoRecorder()
        {
            _captureSource = new CaptureSource();
            _fileSink = new FileSink();


            _videoRecorderBrush = new VideoBrush();
            _videoRecorderBrush.SetSource(_captureSource);

            _captureSource.Start();
        }

        private void ChooseCamera()
        {
            IReadOnlyCollection<VideoCaptureDevice> availableSources = CaptureDeviceConfiguration.GetAvailableVideoCaptureDevices();
            CaptureDeviceConfiguration.RequestDeviceAccess();

            _captureSource.Stop();

            _captureSource.CaptureImageCompleted += _captureSource_CaptureImageCompleted;

            if (availableSources.Count() == 1)
            {
                //there is only one camera!
                foreach (var row in availableSources)
                {
                    _captureSource.VideoCaptureDevice = row;
                }
            }

            if (AppSettings.SelectedCamera == AppSettings.Camera.Front)
            {
                foreach (var row in availableSources)
                {
                    if (row.FriendlyName.ToUpper().Contains("SELF"))
                    {
                        _captureSource.VideoCaptureDevice = row;
                    }
                }
            }
            else
            {
                _captureSource.VideoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();
            }
        }

        void _captureSource_CaptureImageCompleted(object sender, CaptureImageCompletedEventArgs e)
        {

        }

        private string GetFileName()
        {
            string returnValue = string.Empty;
            int storedNumber = 0;
            int currentFileNumber = 0;

            if (AppSettings.SelectedCamera == AppSettings.Camera.Front)
            {
                returnValue = "Front.";
            }
            else
            {
                returnValue = "Back.";
            }

            if (IS.GetSetting(VIDEONUMBER) != null)
            {
                storedNumber = (int)IS.GetSetting(VIDEONUMBER);
            }

            currentFileNumber = storedNumber + 1;

            returnValue += currentFileNumber + ".mp4";

            IS.SaveSetting(VIDEONUMBER, currentFileNumber);

            return returnValue;
        }

        #region click handlers

        private void RecordClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ChooseCamera();

                if (_captureSource.VideoCaptureDevice != null && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();

                    // Disconnect fileSink.
                    _fileSink.CaptureSource = null;
                    _fileSink.IsolatedStorageFileName = null;

                }

                if (_captureSource.VideoCaptureDevice != null && _captureSource.State == CaptureState.Stopped)
                {
                    _fileSink.CaptureSource = _captureSource;
                    _fileSink.IsolatedStorageFileName = GetFileName();

                    _captureSource.Start();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void PreviewClicked(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/VideoReview.xaml", UriKind.Relative));
        }

        private void NavigateWebBrowserClicked(object sender, RoutedEventArgs e)
        {
            WebBrowser.Source = new Uri(Website.Text, UriKind.Absolute);
        }

        private void ShowFilesClicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListOfStoredFiles.xaml", UriKind.Relative));
        }

        private void StopRecordClicked(object sender, EventArgs e)
        {
            try
            {
                // Stop recording.
                if (_captureSource.VideoCaptureDevice != null
                && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();

                    // Disconnect fileSink.
                    _fileSink.CaptureSource = null;
                    _fileSink.IsolatedStorageFileName = null;
                }
            }
            // If stop fails, display an error.
            catch (Exception ex)
            {
                //this.Dispatcher.BeginInvoke(delegate()
                //{
                //    txtDebug.Text = "ERROR: " + e.Message.ToString();
                //});
            }
        }

        private void SettingsClicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void ReviewClicked(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void MoreApplicationsClicked(object sender, EventArgs e)
        {
            MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();

            marketplaceSearchTask.SearchTerms = "KLBCreations";
            marketplaceSearchTask.Show();
        }

        private void BackButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Stop recording.
                if (_captureSource.VideoCaptureDevice != null
                && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();

                    // Disconnect fileSink.
                    _fileSink.CaptureSource = null;
                    _fileSink.IsolatedStorageFileName = null;
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private void CleanStorageClicked(object sender, EventArgs e)
        {
            MessageBoxResult rslt;

            rslt = MessageBox.Show("Do you wish to delete all of your videos?  Warning:  once deleted, they can not be recovered!", "Delete all Videos", MessageBoxButton.OK);

            if (rslt == MessageBoxResult.OK)
            {
                try
                {
                    using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        string[] allFiles = iso.GetFileNames();

                        for (int i = 0; i < allFiles.Count(); i++)
                        {
                            iso.DeleteFile(allFiles[i]);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        #endregion click handlers

    }
}