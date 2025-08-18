using AVFoundation;
using System;

namespace Maui_objC_iOS_Custom_Scanner_Service.Platforms.iOS
{
    public class MetadataOutputDelegate : AVCaptureMetadataOutputObjectsDelegate
    {
        private readonly Action<string> _onDetected;
        private readonly AVCaptureSession _session;
        private bool _hasScanned = false;

        public MetadataOutputDelegate(Action<string> onDetected, AVCaptureSession session)
        {
            _onDetected = onDetected;
            _session = session;
        }

        public override void DidOutputMetadataObjects(AVCaptureMetadataOutput captureOutput, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
        {
            if (_hasScanned)
                return;

            if (metadataObjects != null && metadataObjects.Length > 0)
            {
                var readableObject = metadataObjects[0] as AVMetadataMachineReadableCodeObject;
                var result = readableObject?.StringValue;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    _hasScanned = true;
                    _session?.StopRunning(); // Stop scanning after successful detection
                    _onDetected?.Invoke(result);
                }
            }
        }
    }
}
