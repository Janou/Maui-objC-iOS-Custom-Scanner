namespace Maui_objC_iOS_Custom_Scanner_Service.Services
{
    public interface ICustomScannerService
    {
#if IOS
    void StartScanner(UIKit.UIView previewView, UIKit.UIView scanBoxView, Action<string> onDetected);
#else
        void StartScanner(int x, int y, int width, int height, Action<string> onDetected);
#endif
    }
}