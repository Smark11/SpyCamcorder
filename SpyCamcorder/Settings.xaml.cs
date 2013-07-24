using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Common.IsolatedStoreage;

namespace SpyCamcorder
{
    public partial class Settings : PhoneApplicationPage, INotifyPropertyChanged
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = this;
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                string position = string.Empty;

                if (IS.GetSetting(AppSettings.CAMERAPOSITION) != null)
                {
                    position = (string)IS.GetSetting(AppSettings.CAMERAPOSITION);
                }

                if (position == string.Empty)
                {
                    RearCameraSelected = true;
                }
                else
                {
                    if (position.ToUpper().Equals(AppSettings.FRONT))
                    {
                        FrontCameraSelected = true;
                        RearCameraSelected = false;
                    }
                    else
                    {
                        FrontCameraSelected = false;
                        RearCameraSelected = true;
                    }
                }

                if (IS.GetSetting(AppSettings.LOCKSCREENDISABLED) != null)
                {
                    LockScreenDisabled = (bool)IS.GetSetting(AppSettings.LOCKSCREENDISABLED);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private bool _rearCameraSelected;
        public bool RearCameraSelected
        {
            get { return _rearCameraSelected; }
            set
            {
                _rearCameraSelected = value; RaisePropertyChanged("RearCameraSelected");
                if (value)
                {
                    IS.SaveSetting(AppSettings.CAMERAPOSITION, AppSettings.REAR);
                    AppSettings.SelectedCamera = AppSettings.Camera.Rear;
                }
            }
        }


        private bool _frontCameraSelected;
        public bool FrontCameraSelected
        {
            get { return _frontCameraSelected; }
            set
            {
                _frontCameraSelected = value;
                RaisePropertyChanged("FrontCameraSelected");
                if (value)
                {
                    IS.SaveSetting(AppSettings.CAMERAPOSITION, AppSettings.FRONT);
                    AppSettings.SelectedCamera = AppSettings.Camera.Front;
                }
            }
        }

        private bool _lockScreenDisabled;
        public bool LockScreenDisabled
        {
            get { return _lockScreenDisabled; }
            set
            {
                _lockScreenDisabled = value;
                RaisePropertyChanged("LockScreenDisabled");
                IS.SaveSetting(AppSettings.LOCKSCREENDISABLED, value);
                AppSettings.LockScreenDisabled = value;
            }
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