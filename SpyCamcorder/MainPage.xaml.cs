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

namespace SpyCamcorder
{
    public partial class MainPage : PhoneApplicationPage
    {
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
                InitializeComponent();

                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

                WebBrowser.Source = new Uri("http://www.espn.com/mobile", UriKind.Absolute);

                // Sample code to localize the ApplicationBar
                //BuildLocalizedApplicationBar();
                InitalizeVideoRecorder();
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
            //if (_captureSource.VideoCaptureDevice != null)
            //{
            //    _captureSource.VideoCaptureDevice = null;
            //}

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

        private void SkyDriveClicked(object sender, RoutedEventArgs e)
        {
            Tryit();
        }

        private async void Tryit()
        {

            var reqList = BackgroundTransferService.Requests.ToList();

            foreach (var req in reqList)
            {
                BackgroundTransferService.Remove(BackgroundTransferService.Find(req.RequestId));
            }

            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fileStream = store.OpenFile(_isoVideoFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.Read))
                    {
                        store.CreateDirectory("/shared/transfers");
                        store.CopyFile(_isoVideoFileName, "/shared/transfers/" + _isoVideoFileName, true);

                        LiveAuthClient authClient = new LiveAuthClient("0000000040101617");
                        LiveLoginResult authResult = await authClient.LoginAsync(new List<string>() { "wl.basic", "wl.skydrive", "wl.skydrive_update" });

                        LiveConnectClient myClient = new LiveConnectClient(authResult.Session);
                        LiveOperationResult result = await myClient.GetAsync("me/skydrive");

                        LiveConnectClient uploadClient = new LiveConnectClient(authResult.Session);
                        LiveOperationResult updateLoadResult = await uploadClient.BackgroundUploadAsync("me/skydrive", new Uri("/shared/transfers/" + _isoVideoFileName, UriKind.Relative), OverwriteOption.Overwrite);
                        //LiveOperationResult opResult = await uploadClient.UploadAsync("me/skydrive", _isoVideoFile,fileStream.AsOutputStream, OverwriteOption.Overwrite);

                        dynamic dResult = result.Result;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private string GetFileName()
        {
            string returnValue = string.Empty;
            int storedNumber = 0;
            int currentFileNumber = 0;

            if (IS.GetSetting(VIDEONUMBER) != null)
            {
                storedNumber = (int)IS.GetSetting(VIDEONUMBER);
            }

            currentFileNumber = storedNumber + 1;

            returnValue = "SpyCamera" + currentFileNumber + ".mp4";

            IS.SaveSetting(VIDEONUMBER, currentFileNumber);

            return returnValue;
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
    }
}