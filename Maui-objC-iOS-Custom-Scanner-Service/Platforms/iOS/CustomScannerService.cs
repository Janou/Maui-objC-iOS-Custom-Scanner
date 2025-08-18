using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using UIKit;

[assembly: Dependency(typeof(Maui_objC_iOS_Custom_Scanner_Service.Platforms.iOS.CustomScannerService))]
namespace Maui_objC_iOS_Custom_Scanner_Service.Platforms.iOS
{
    public class CustomScannerService : ICustomScannerService
    {
        AVCaptureSession captureSession;
        AVCaptureMetadataOutput metadataOutput;
        AVCaptureVideoPreviewLayer previewLayer;

        Action<string> onResultCallback;

        public void StartScanner(UIView previewView, UIView scanBoxView, Action<string> onDetected)
        {
            onResultCallback = onDetected;

            captureSession = new AVCaptureSession();

            var videoCaptureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (videoCaptureDevice == null)
            {
                Console.WriteLine("❌ No video device found.");
                return;
            }

            var videoInput = AVCaptureDeviceInput.FromDevice(videoCaptureDevice, out var inputError);
            if (inputError != null)
            {
                Console.WriteLine($"❌ Error creating input: {inputError.LocalizedDescription}");
                return;
            }

            if (captureSession.CanAddInput(videoInput))
                captureSession.AddInput(videoInput);

            metadataOutput = new AVCaptureMetadataOutput();
            if (captureSession.CanAddOutput(metadataOutput))
                captureSession.AddOutput(metadataOutput);

            // Set the scanning area
            var rectOfInterest = GetRectOfInterest(scanBoxView.Frame, previewView);
            metadataOutput.RectOfInterest = rectOfInterest;

            // Set metadata types using helper
            var typesArray = NSArray.FromObjects(
                new NSString("org.iso.QRCode"),
                new NSString("org.gs1.EAN-13"),
                new NSString("org.gs1.EAN-8"),
                new NSString("org.iso.Code128"),
                new NSString("org.gs1.UPC-E"),
                new NSString("org.iso.Code39"),
                new NSString("org.iso.PDF417")
            );
            ObjCRuntimeHelper.SendMessage(metadataOutput.Handle, "setMetadataObjectTypes:", typesArray.Handle);

            // Set delegate for handling detected codes
            metadataOutput.SetDelegate(new MetadataOutputDelegate(onResultCallback, captureSession), DispatchQueue.MainQueue);

            // Create and configure the preview layer
            previewLayer = new AVCaptureVideoPreviewLayer(captureSession)
            {
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill,
                Frame = previewView.Bounds
            };

            // Clean up old layers to avoid duplicates
            previewView.Layer.Sublayers?.ToList().ForEach(layer => layer.RemoveFromSuperLayer());

            previewView.Layer.InsertSublayer(previewLayer, 0);

            captureSession.StartRunning();
            Console.WriteLine("🎥 Scanner started.");
        }

        CGRect GetRectOfInterest(CGRect boundingBoxFrame, UIView previewView)
        {
            var screenSize = previewView.Bounds.Size;

            // Convert bounding box to normalized rect
            var x = boundingBoxFrame.Y / screenSize.Height;
            var y = 1.0 - ((boundingBoxFrame.X + boundingBoxFrame.Width) / screenSize.Width);
            var width = boundingBoxFrame.Height / screenSize.Height;
            var height = boundingBoxFrame.Width / screenSize.Width;

            return new CGRect(x, y, width, height);
        }

        class MetadataOutputDelegate : AVCaptureMetadataOutputObjectsDelegate
        {
            readonly Action<string> onResult;
            readonly AVCaptureSession session;

            public MetadataOutputDelegate(Action<string> onResult, AVCaptureSession session)
            {
                this.onResult = onResult;
                this.session = session;
            }

            public override void DidOutputMetadataObjects(AVCaptureMetadataOutput output, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
            {
                if (metadataObjects.Length > 0)
                {
                    var obj = metadataObjects[0] as AVMetadataMachineReadableCodeObject;
                    if (obj != null && !string.IsNullOrEmpty(obj.StringValue))
                    {
                        session.StopRunning();
                        onResult?.Invoke(obj.StringValue);
                    }
                }
            }
        }
    }
}
