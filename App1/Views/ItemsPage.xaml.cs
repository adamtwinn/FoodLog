using App1.Helpers;
using FoodLog.Common;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsPage
    {
        private static readonly MainViewModel _viewModel;

        static ItemsPage()
        {
            _viewModel = new MainViewModel(DependencyService.Get<IApiWrapper>());
        }

        public ItemsPage()
        {
            InitializeComponent();

            try
            {
                Messenger.Instance.Register<Notification>("Notification", async n => { await DisplayMessage(n); });
                Messenger.Instance.Register<Exception>("Exception", async ex => { await DisplayException(ex); });
                
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                Reporter.ReportException(ex);
            }
        }

        private async Task DisplayMessage(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.Cancel))
                await DisplayAlert(notification.Title, notification.Message, notification.Accept);
            else
                await DisplayAlert(notification.Title, notification.Message, notification.Accept, notification.Cancel);

        }

        private async Task DisplayException(Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }

        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                //if (_viewModel.Entries.Count == 0)
                await _viewModel.Start();
            }
            catch (Exception ex)
            {
                Reporter.ReportException(ex);
            }
        }
    }
}