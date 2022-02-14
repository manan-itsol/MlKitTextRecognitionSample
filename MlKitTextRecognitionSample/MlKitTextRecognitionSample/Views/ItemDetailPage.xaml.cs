using MlKitTextRecognitionSample.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace MlKitTextRecognitionSample.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}