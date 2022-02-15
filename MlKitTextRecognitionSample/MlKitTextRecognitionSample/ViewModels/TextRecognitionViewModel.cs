using MlKitTextRecognitionSample.Services;
using Plugin.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MlKitTextRecognitionSample.ViewModels
{
    public class TextRecognitionViewModel : BaseViewModel
    {
        public TextRecognitionViewModel()
        {
            Title = "About";
            CaptureCommand = new Command(OnCaptureClicked);
        }

        public Command CaptureCommand { get; }

        private string _extractedText;
        public string ExtractedText
        {
            get => _extractedText;
            set
            {
                SetProperty(ref _extractedText, value);
            }
        }

        private async void OnCaptureClicked()
        {
            IsBusy = true;
            byte[] imageData = null;
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
                Directory = "Sample",
                Name = "ATM_Receipt.jpg",
                RotateImage = Device.RuntimePlatform != Device.iOS,
                // MaxWidthHeight = maxWidthHeight,
                DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Rear,

                //CompressionQuality = quality,
                AllowCropping = true,
                ModalPresentationStyle = Plugin.Media.Abstractions.MediaPickerModalPresentationStyle.FullScreen,
            });
            if (file == null)
                return;
            Stream stream = file.GetStream();
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }
            await ExtractTextMlKit(imageData);
        }

        private async Task ExtractTextMlKit(byte[] image)
        {
            #region ml-kit text recognition
            // google ml kit text recognition commented
            var ocrExtractor = DependencyService.Get<IOcrExtractor>();
            var mlResult = await ocrExtractor.ProcessImageAsync(image);
            ExtractedText = mlResult;
            await Clipboard.SetTextAsync(ExtractedText);
            #endregion ml-kit text recognition
        }
    }
}