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
        public static string ISSECONDTIMEOPENED = "ISSECONDTIMEOPENED";
        public static string ISAPPRATED = "ISAPPRATED";

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

        public static bool IsSecondTimeOpen { get; set; }
        public static bool IsAppRated { get; set; }

        public static void Initialize()
        {

        }

        public static void SetAppAsRated()
        {
            IS.SaveSetting(ISAPPRATED, true);
            IsAppRated = true;
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

            if (IS.GetSetting(ISSECONDTIMEOPENED) == null)
            {
                IsSecondTimeOpen = false;
                IS.SaveSetting(ISSECONDTIMEOPENED, true);
            }
            else
            {
                IsSecondTimeOpen = true;
            }

            if (IS.GetSetting(ISAPPRATED) != null)
            {
                bool rated = (bool)IS.GetSetting(ISAPPRATED);
                IsAppRated = rated;
            }
        }

        public static void ToggleLockScreen(IdleDetectionMode mode)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = mode;
        }
    }
}
