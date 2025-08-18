using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using AndroidX.Lifecycle;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using Google.MLKit.Vision.Barcode;
using Google.MLKit.Vision.Common;
using Microsoft.Maui.Controls.Platform;
using Maui_objC_iOS_Custom_Scanner_Service.Services;

namespace Maui_objC_iOS_Custom_Scanner_Service.Platforms.Android;

public class BarcodeScannerService_Android : IBarcodeScannerService
{
    private PreviewView _previewView;
    private IProcessCameraProvider _cameraProvider;
    private Action<string> _onResult;
    private AndroidX.ConstraintLayout.Core.Motion.Utils.Rect _boundingBox;

    public async Task StartScanningAsync(AndroidX.ConstraintLayout.Core.Motion.Utils.Rect boundingBox, Action<string> onResult)
    {
        _onResult = onResult;
        _boundingBox = boundingBox;

        var context = Platform.CurrentActivity;
        _previewView = new PreviewView(context)
        {
            LayoutParameters = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
        };

        var rootLayout = new FrameLayout(context);
        rootLayout.AddView(_previewView);

        var layout = new Android.App.Dialog(context);
        layout.SetContentView(rootLayout);
        layout.Show();

        var cameraProviderFuture = ProcessCameraProvider.GetInstance(context);
        _cameraProvider = (IProcessCameraProvider)await cameraProviderFuture.AsAsync<ProcessCameraProvider>();

        var cameraSelector = new CameraSelector.Builder()
            .RequireLensFacing(CameraSelector.LensFacingBack)
            .Build();

        var preview = new Preview.Builder().Build();
        preview.SetSurfaceProvider(_previewView.SurfaceProvider);

        var analysis = new ImageAnalysis.Builder()
            .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
            .Build();

        var barcodeScanner = BarcodeScanning.GetClient(
            new BarcodeScannerOptions.Builder()
                .SetBarcodeFormats(Barcode.FormatAllFormats)
                .Build());

        analysis.SetAnalyzer(ContextCompat.MainExecutor(context), new BarcodeAnalyzer(barcodeScanner, _boundingBox, onResult, layout, this));

        _cameraProvider.UnbindAll();
        _cameraProvider.BindToLifecycle((ILifecycleOwner)context, cameraSelector, preview, analysis);
    }

    public void StopScanning()
    {
        _cameraProvider?.UnbindAll();
    }

    private class BarcodeAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
    {
        private readonly IBarcodeScanner _scanner;
        private readonly Rect _roi;
        private readonly Action<string> _callback;
        private readonly Dialog _dialog;
        private readonly BarcodeScannerService_Android _service;

        public BarcodeAnalyzer(IBarcodeScanner scanner, Rect roi, Action<string> callback, Dialog dialog, BarcodeScannerService_Android service)
        {
            _scanner = scanner;
            _roi = roi;
            _callback = callback;
            _dialog = dialog;
            _service = service;
        }

        public void Analyze(IImageProxy image)
        {
            var mediaImage = image.Image;
            if (mediaImage == null)
            {
                image.Close();
                return;
            }

            var inputImage = InputImage.FromMediaImage(mediaImage, image.ImageInfo.RotationDegrees);

            _scanner.Process(inputImage)
                .AddOnSuccessListener(new OnSuccessListener(barcodes =>
                {
                    foreach (var barcode in barcodes)
                    {
                        // Note: ML Kit doesn't guarantee barcode.BoundingBox is set, but usually is
                        var bounds = barcode.BoundingBox;

                        if (bounds != null && _roi.Contains(bounds))
                        {
                            _service.StopScanning();
                            _dialog.Dismiss();
                            _callback(barcode.RawValue);
                            break;
                        }
                    }

                    image.Close();
                }))
                .AddOnFailureListener(new OnFailureListener(ex =>
                {
                    System.Diagnostics.Debug.WriteLine("Scanner error: " + ex.Message);
                    image.Close();
                }));
        }
    }
}
