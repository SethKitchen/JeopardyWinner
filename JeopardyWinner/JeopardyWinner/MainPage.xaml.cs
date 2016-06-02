using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace JeopardyWinner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //ApplicationView.PreferredLaunchWindowingMode =ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void GoGoogURL_Click(object sender, RoutedEventArgs e)
        {
            googWindows.Navigate(new Uri(GoogURL.Text));
        }

        private void GoJeoURL_Click(object sender, RoutedEventArgs e)
        {
            jeoWindow.Navigate(new Uri(JeoURL.Text));
        }

        public async void captureJeoAndDoOCR()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + @"\Images\Jeo.jpg");
                await SaveToFileAsync(jeoWindow, file);
                SoftwareBitmap bitmap = null;
                //StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"\Images\Jeo.jpg");
                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                    var imgSource = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                    bitmap.CopyToBuffer(imgSource.PixelBuffer);
                }

                OcrEngine ocrEngine = null;

                // Try to create OcrEngine for specified language.
                // If language is not supported on device, method returns null.
                ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en-US"));

                // Check if OcrEngine supports image resoulution.
                if (bitmap.PixelWidth > OcrEngine.MaxImageDimension || bitmap.PixelHeight > OcrEngine.MaxImageDimension)
                {
                    return;
                }

                if (ocrEngine != null)
                {
                    // Recognize text from image.
                    var ocrResult = await ocrEngine.RecognizeAsync(bitmap);

                    string query = ocrResult.Text.Replace(' ', '+');
                    // Display recognized text.
                    GoogURL.Text = "https://www.google.com/?gws_rd=ssl#q=" + query;
                    GoGoogURL_Click(this, new RoutedEventArgs());
                }
            }
            catch (Exception e)
            {

            }
        }

        async Task<RenderTargetBitmap> SaveToFileAsync(FrameworkElement uielement, StorageFile file)
        {
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                Guid encoderId = GetBitmapEncoder(file.FileType);

                try
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        return await CaptureToStreamAsync(uielement, stream, encoderId);
                    }
                }
                catch (Exception ex)
                {
                    //DisplayMessage(ex.Message);
                }

                var status = await CachedFileManager.CompleteUpdatesAsync(file);
            }

            return null;
        }

        async Task<RenderTargetBitmap> CaptureToStreamAsync(FrameworkElement uielement, IRandomAccessStream stream, Guid encoderId)
        {
            try
            {
                var renderTargetBitmap = new RenderTargetBitmap();
                await renderTargetBitmap.RenderAsync(uielement);

                var pixels = await renderTargetBitmap.GetPixelsAsync();

                var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                var encoder = await BitmapEncoder.CreateAsync(encoderId, stream);
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    logicalDpi,
                    logicalDpi,
                    pixels.ToArray());

                await encoder.FlushAsync();

                return renderTargetBitmap;
            }
            catch (Exception ex)
            {
                //DisplayMessage(ex.Message);
            }

            return null;
        }

        Guid GetBitmapEncoder(string fileType)
        {
            Guid encoderId = BitmapEncoder.JpegEncoderId;
            switch (fileType)
            {
                case ".bmp":
                    encoderId = BitmapEncoder.BmpEncoderId;
                    break;
                case ".gif":
                    encoderId = BitmapEncoder.GifEncoderId;
                    break;
                case ".png":
                    encoderId = BitmapEncoder.PngEncoderId;
                    break;
                case ".tif":
                    encoderId = BitmapEncoder.TiffEncoderId;
                    break;
            }

            return encoderId;
        }

        private void DoOCRAndSearch_Click(object sender, RoutedEventArgs e)
        {
            captureJeoAndDoOCR();
        }

        private void jeoWindow_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            googWindows.Navigate(args.Uri);
            GoogURL.Text = args.Uri.ToString();
            args.Handled = true;
        }
    }
}
