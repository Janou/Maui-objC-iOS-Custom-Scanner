using Android.App;
using Android.Content;
using Android.Runtime;
using Com.Skupad.Scanner; // This is your binding namespace
using Microsoft.Maui.Controls.Platform;

[assembly: Dependency(typeof(Maui_objC_iOS_Custom_Scanner_Service.Platforms.Android.Services.CustomScannerService))]
namespace Maui_objC_iOS_Custom_Scanner_Service.Platforms.Android.Services
{
    public class CustomScannerService : Java.Lang.Object, ICustomScannerService, CustomScanner.IResultListener
    {
        private Action<string>? _onResult;

        public void StartScanner(int x, int y, int width, int height, Action<string> onResult)
        {
            _onResult = onResult;

            var activity = Platform.CurrentActivity ?? throw new InvalidOperationException("No current activity");

            CustomScanner.StartScanner(
                activity,
                x, y, width, height,
                this
            );
        }

        public void OnScanned(string result)
        {
            // Dispatch back to .NET MAUI thread
            MainThread.BeginInvokeOnMainThread(() => _onResult?.Invoke(result));
        }
    }
}
