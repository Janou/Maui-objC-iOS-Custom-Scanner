using AVFoundation;
using Foundation;

namespace Maui_objC_iOS_Custom_Scanner_Service.Platforms.iOS
{
    public static class TorchService
    {
        private static AVCaptureDevice GetCameraDevice()
        {
            var discoverySession = AVCaptureDeviceDiscoverySession.Create(
                new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera },
                AVMediaTypes.Video,
                AVCaptureDevicePosition.Back);

            return discoverySession?.Devices?.FirstOrDefault();
        }

        public static void ToggleTorch()
        {
            var device = GetCameraDevice();

            if (device == null || !device.HasTorch)
                return;

            NSError error;
            device.LockForConfiguration(out error);
            if (error == null)
            {
                device.TorchMode = device.TorchMode == AVCaptureTorchMode.On
                    ? AVCaptureTorchMode.Off
                    : AVCaptureTorchMode.On;

                device.UnlockForConfiguration();
            }
        }

        public static void SetTorch(bool enable)
        {
            var device = GetCameraDevice();

            if (device == null || !device.HasTorch)
                return;

            NSError error;
            device.LockForConfiguration(out error);
            if (error == null)
            {
                device.TorchMode = enable ? AVCaptureTorchMode.On : AVCaptureTorchMode.Off;
                device.UnlockForConfiguration();
            }
        }
    }
}
