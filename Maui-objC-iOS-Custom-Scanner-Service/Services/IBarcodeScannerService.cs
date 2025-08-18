namespace Maui_objC_iOS_Custom_Scanner_Service.Services
{
    public interface IBarcodeScannerService
    {
        Task StartScanningAsync(Rect boundingBox, Action<string> onResult);
        void StopScanning();
    }
}
