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
using System.Threading.Tasks;

namespace SpyCamcorder
{
    public partial class MainPage : PhoneApplicationPage
    {
        public enum RecordState
        {
            ReadyToRecord,
            Recording,
            StartingToRecord
        }

        public static MainPage _mainPageInstance;
        private const string VIDEONUMBER = "videonumber";
        private const string STOREDWEBSITE = "storedwebsite";
        private RecordState _currentState;

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

                BuildAppBar(AppSettings.IsAppRated);

                GoToWebsite();

                // Sample code to localize the ApplicationBar
                //BuildLocalizedApplicationBar();
                InitalizeVideoRecorder();

                IsAppTrialOrBought(false);
            }
            catch (Exception ex)
            {

            }
        }

        private void BuildAppBar(bool appHasBeenRated)
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;

            // Settings
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.settings.png", UriKind.Relative));
            appBarButton.Text = AppResources.Settings;
            ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += new EventHandler(SettingsClicked);

            //Videos
            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Assets/AppBar/folder.png", UriKind.Relative));
            appBarButton2.Text = AppResources.Videos;
            ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += new EventHandler(ShowFilesClicked);

            //Rate
            if (!appHasBeenRated)
            {
                ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("/Assets/AppBar/favs.png", UriKind.Relative));
                appBarButton3.Text = AppResources.Rate;
                ApplicationBar.Buttons.Add(appBarButton3);
                appBarButton3.Click += new EventHandler(ReviewClicked);
            }

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.MoreAps);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
            appBarMenuItem.Click += new EventHandler(MoreApplicationsClicked);

            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.DeleteAllVideos1);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            appBarMenuItem2.Click += new EventHandler(CleanStorageClicked);

            ApplicationBarMenuItem appBarMenuItem4 = new ApplicationBarMenuItem(AppResources.Instructions);
            ApplicationBar.MenuItems.Add(appBarMenuItem4);
            appBarMenuItem4.Click += new EventHandler(InstructionsClicked);

            ApplicationBarMenuItem appBarMenuItem5 = new ApplicationBarMenuItem(AppResources.About);
            ApplicationBar.MenuItems.Add(appBarMenuItem5);
            appBarMenuItem5.Click += new EventHandler(AboutClicked);
        }

        private void GoToWebsite()
        {
            if (IS.GetSetting(STOREDWEBSITE) == null)
            {
                Website.Text = "http://www.windowsphone.com/en-us/search?q=klbcreations";
                WebBrowser.Source = new Uri("http://www.windowsphone.com/en-us/search?q=klbcreations", UriKind.Absolute);
            }
            else
            {
                WebBrowser.Source = new Uri((string)IS.GetSetting(STOREDWEBSITE), UriKind.Absolute);
                Website.Text = (string)IS.GetSetting(STOREDWEBSITE);
            }
        }

        public void IsAppTrialOrBought(bool reactivated)
        {
            try
            {
                if ((Application.Current as App).IsTrial)
                {
                    Trial.SaveStartDateOfTrial();

                    if (Trial.IsTrialExpired())
                    {
                        MessageBox.Show(AppResources.TrialExpired);
                        RecordButton.IsEnabled = false;
                        MarketplaceDetailTask task = new MarketplaceDetailTask();
                        task.Show();
                        InitializeAppForTrialOrActivated(false);
                    }
                    else
                    {
                        if (AppSettings.IsSecondTimeOpen && !AppSettings.IsAppRated)
                        {
                            if (!reactivated)
                            {
                                MessageBoxResult msgResult;
                                msgResult = MessageBox.Show(AppResources.TrialLeft1 + Trial.GetDaysLeftInTrial() + AppResources.TrialLeft2, AppResources.TrialLeft3, MessageBoxButton.OKCancel);
                                if (msgResult == MessageBoxResult.OK)
                                {
                                    Trial.Add10DaysToTrial();
                                    AppSettings.SetAppAsRated();
                                    MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                                    marketplaceReviewTask.Show();
                                }
                            }
                        }
                        else
                        {
                            if (!reactivated)
                            {
                                MessageBoxResult msgResult;

                                msgResult = MessageBox.Show(AppResources.TrialLeft1 + Trial.GetDaysLeftInTrial() + AppResources.TrialLeft4, AppResources.TrialLeft5, MessageBoxButton.OKCancel);

                                if (msgResult == MessageBoxResult.OK)
                                {
                                    AppSettings.SetAppAsRated();
                                    MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                                    marketplaceReviewTask.Show();
                                }
                            }
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
            try
            {
                _captureSource = new CaptureSource();
                _fileSink = new FileSink();


                _videoRecorderBrush = new VideoBrush();
                _videoRecorderBrush.SetSource(_captureSource);

                _captureSource.Start();
                SetRecordState(RecordState.ReadyToRecord);
            }
            catch (Exception ex)
            {

            }
        }

        public void SetRecordState(RecordState state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    switch (state)
                    {
                        case RecordState.ReadyToRecord:
                            RecordBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                            //RecordButton.IsEnabled = true;
                            _currentState = RecordState.ReadyToRecord;
                            break;
                        case RecordState.StartingToRecord:
                            RecordBorder.BorderBrush = new SolidColorBrush(Colors.Yellow);
                            //RecordButton.IsEnabled = false;
                            _currentState = RecordState.StartingToRecord;
                            break;
                        case RecordState.Recording:
                            RecordBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                            //RecordButton.IsEnabled = true;
                            _currentState = RecordState.Recording;
                            break;
                    }
                });
        }

        private void ChooseCamera()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred.  Please try again.");
            }
        }

        void _captureSource_CaptureImageCompleted(object sender, CaptureImageCompletedEventArgs e)
        {

        }

        private string GetFileName()
        {
            string returnValue = string.Empty;

            try
            {
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
            }
            catch (Exception ex)
            {

            }

            return returnValue;
        }

        #region click handlers

        private void RecordClicked(object sender, RoutedEventArgs e)
        {
            if (_currentState == RecordState.Recording)
            {
                StopRecording();
            }
            else
            {
                SetRecordState(RecordState.StartingToRecord);
                Task.Factory.StartNew(() => StartRecording());
            }
        }

        private void PreviewClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/VideoReview.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {

            }
        }

        private void InstructionsClicked(object sender, EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/Instructions.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {

            }
        }

        private void NavigateWebBrowserClicked(object sender, RoutedEventArgs e)
        {
            NavigateToWebPage();
        }

        private void NavigateToWebPage()
        {
            try
            {
                if (!Website.Text.ToUpper().StartsWith("HTTP://"))
                {
                    string text = Website.Text;
                    Website.Text = "http://" + text;
                }
                WebBrowser.Source = new Uri(Website.Text, UriKind.Absolute);
                IS.SaveSetting(STOREDWEBSITE, Website.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void ShowFilesClicked(object sender, EventArgs e)
        {
            try
            {
                if (_currentState == RecordState.ReadyToRecord)
                {
                    NavigationService.Navigate(new Uri("/ListOfStoredFiles.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show(AppResources.StopRecording);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void StartRecording()
        {
            try
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(.05));
                Dispatcher.BeginInvoke(() =>
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
                    });


                SetRecordState(RecordState.Recording);
            }
            catch (Exception ex)
            {
                SetRecordState(RecordState.ReadyToRecord);
            }
        }

        private void StopRecording()
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
                SetRecordState(RecordState.ReadyToRecord);
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

        private void StopRecordClicked(object sender, EventArgs e)
        {

        }

        private void SettingsClicked(object sender, EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {

            }
        }

        private void ReviewClicked(object sender, EventArgs e)
        {
            try
            {
                MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                marketplaceReviewTask.Show();
            }
            catch (Exception ex)
            {

            }
        }

        private void MoreApplicationsClicked(object sender, EventArgs e)
        {
            try
            {
                MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();

                marketplaceSearchTask.SearchTerms = "KLBCreations";
                marketplaceSearchTask.Show();
            }
            catch (Exception ex)
            {

            }
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

            rslt = MessageBox.Show(AppResources.DeleteAllVideos, AppResources.DeleteAllVideos1, MessageBoxButton.OKCancel);

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

        private void AboutClicked(object sender, EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {

            }
        }

        private void WebBrowserBoxGotFocus(object sender, RoutedEventArgs e)
        {
            Website.SelectAll();
        }

        private void WebSiteUrlKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                NavigateToWebPage();

                this.Focus();
            }
        }

        #endregion click handlers
    }
}