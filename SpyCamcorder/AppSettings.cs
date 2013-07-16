using Common.IsolatedStoreage;
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

        public enum Camera
        {
            Rear,
            Front
        }

        public static Camera SelectedCamera { get; set; }

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
        }
    }
}
