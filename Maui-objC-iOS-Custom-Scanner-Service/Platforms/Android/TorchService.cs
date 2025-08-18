using Android.Content;
using Android.Hardware.Camera2;
using Android.Util;
using Java.Lang;
using Android.OS;
using AndroidApp = Android.App.Application;

namespace Maui_objC_iOS_Custom_Scanner_Service.Platforms.Android
{
    public class TorchService
    {
        CameraManager cameraManager;
        string cameraId;

        public TorchService()
        {
            cameraManager = (CameraManager)AndroidApp.Context.GetSystemService(Context.CameraService);
            try
            {
                foreach (var id in cameraManager.GetCameraIdList())
                {
                    var characteristics = cameraManager.GetCameraCharacteristics(id);
                    var flashAvailable = (Java.Lang.Boolean)characteristics.Get(CameraCharacteristics.FlashInfoAvailable);
                    var lensFacing = (Integer)characteristics.Get(CameraCharacteristics.LensFacing);

                    if (flashAvailable != null && flashAvailable.BooleanValue() &&
                        lensFacing != null && lensFacing.IntValue() == (int)LensFacing.Back)
                    {
                        cameraId = id;
                        break;
                    }
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Error("TorchService", $"Error finding camera: {ex.Message}");
            }
        }

        public void ToggleTorch(bool turnOn)
        {
            try
            {
                if (!string.IsNullOrEmpty(cameraId))
                {
                    cameraManager.SetTorchMode(cameraId, turnOn);
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Error("TorchService", $"Error toggling torch: {ex.Message}");
            }
        }
    }
}
