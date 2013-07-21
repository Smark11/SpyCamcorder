using Common.IsolatedStoreage;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpyCamcorder
{
    public static class AppSettings
    {
        public static string CAMERAPOSITION = "CAMERAPOSITION";
        public static string REAR = "REAR";
        public static string FRONT = "FRONT";
        public static string LOCKSCREENDISABLED = "LOCKSCREENDISABLED";

        public enum Camera
        {
            Rear,
            Front
        }

        public static Camera SelectedCamera { get; set; }

        public static bool _lockScreenDisabled;
        public static bool LockScreenDisabled 
        { 
            get { return _lockScreenDisabled;} 
            set 
            { 
                _lockScreenDisabled = value;
                if (value)
                {
                    ToggleLockScreen(IdleDetectionMode.Disabled);
                }
                else
                {
                    ToggleLockScreen(IdleDetectionMode.Enabled);
                }
            } 
        }

        public static void Initialize()
        {

        }

        static AppSettings()
        {
            if (IS.GetSetting(CAMERAPOSITION) != null)
            {
                string value = (string)IS.GetSetting(CAMERAPOSITION);

                if (value == REAR)
                {
                    SelectedCamera = Camera.Rear;
                }
                else
                {
                    SelectedCamera = Camera.Front;
                }
            }
            else
            {
                SelectedCamera = Camera.Rear;
            }

            if (IS.GetSetting(LOCKSCREENDISABLED) != null)
            {
                LockScreenDisabled = (bool)IS.GetSetting(LOCKSCREENDISABLED);
            }
            else
            {
                LockScreenDisabled = false;
            }
        }

        public static void ToggleLockScreen(IdleDetectionMode mode)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = mode;
        }
    }
}
