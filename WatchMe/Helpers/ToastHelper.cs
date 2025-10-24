using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace WatchMe.Helpers
{
    public static class ToastHelper
    {

        public static async Task CreateToast(string message, ToastDuration duration = ToastDuration.Long)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            double fontSize = 14;

            var toast = Toast.Make(message, duration, fontSize);

            await toast.Show(cancellationTokenSource.Token);
        }
    }
}
