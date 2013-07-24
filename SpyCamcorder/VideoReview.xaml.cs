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
using System.IO;
using Microsoft.Live;
using Microsoft.Phone.BackgroundTransfer;
using System.Diagnostics;
using System.Threading;
using Microsoft.Phone.Net.NetworkInformation;
using System.ComponentModel;

namespace SpyCamcorder
{
    public partial class VideoReview : PhoneApplicationPage, INotifyPropertyChanged
    {
        IsolatedStorageFileStream _isoStream;
        private string _fileName; 

        public VideoReview()
        { 
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                string parameter = string.Empty;

                if (NavigationContext.QueryString.TryGetValue("parameter", out parameter))
                {
                    _fileName = parameter;
                }

                if (_fileName.ToUpper().Contains("BACK"))
                {
                    RotateTransform.Angle = 90;
                }
                else
                {
                    RotateTransform.Angle = 270;
                }

                if (_fileName != string.Empty)
                {
                    _isoStream = new IsolatedStorageFileStream(_fileName, FileMode.Open, FileAccess.Read, IsolatedStorageFile.GetUserStoreForApplication());

                    VideoPlayer.SetSource(_isoStream);
                    VideoPlayer.MediaEnded += VideoPlayer_MediaEnded;
                    VideoPlayer.Play();
                }
            }
            catch (Exception ex)
            {

            }

            base.OnNavigatedTo(e);
        }

        void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //NavigationService.GoBack();
        }

        private void StopHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoPlayer.Stop();
            }
            catch (Exception ex)
            {

            }
        }

        private void PlayHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoPlayer.Play();
            }
            catch (Exception ex)
            {

            }
        }

        private void PauseHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoPlayer.Pause();
            }
            catch (Exception ex)
            {

            }
        }

        private IProgress<LiveOperationProgress> _uploadProgress;

        private async void SkyDriveHandler(object sender, RoutedEventArgs e)
        {
            if (DeviceNetworkInformation.IsWiFiEnabled)
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
                        using (var fileStream = store.OpenFile(_fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.Read))
                        {
                            //store.CreateDirectory("/shared/transfers");
                            //store.CopyFile(_fileName, "/shared/transfers/" + _fileName, true);

                            LiveAuthClient authClient = new LiveAuthClient("0000000040101617");
                            LiveLoginResult authResult = await authClient.LoginAsync(new List<string>() { "wl.basic", "wl.skydrive", "wl.skydrive_update" });

                            LiveConnectClient myClient = new LiveConnectClient(authResult.Session);
                            LiveOperationResult result = await myClient.GetAsync("me/skydrive");

                            Progress<LiveOperationProgress> uploadProgress = new Progress<LiveOperationProgress>((p) =>
                            { Debug.WriteLine("{0}/{1} bytes uploaded ({2}%)", p.BytesTransferred.ToString("0,0"), p.TotalBytes.ToString("0,0"), p.ProgressPercentage.ToString("0,0")); });

                            LiveConnectClient uploadClient = new LiveConnectClient(authResult.Session);
                            LiveOperationResult updateLoadResult = await uploadClient.BackgroundUploadAsync("me/skydrive", new Uri("/shared/transfers/" + _fileName, UriKind.Relative), OverwriteOption.Overwrite, CancellationToken.None, uploadProgress);
                            //LiveOperationResult opResult = await uploadClient.UploadAsync("me/skydrive", _isoVideoFile,fileStream.AsOutputStream, OverwriteOption.Overwrite);


                            dynamic dResult = result.Result;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                MessageBox.Show("Please connect to a wifi network in order to upload videos.");
            }
        }

        private int _rotateAngle;
        public int RotateAngle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; RaisePropertyChanged("RotateAngle"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}