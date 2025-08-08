
#if ANDROID
using Android.Telephony;
#endif
using WatchMe.Config;
using WatchMe.Helpers;

namespace WatchMe.Services
{
    public interface INotificationService
    {
        Task SendTextToConfiguredContact();
    }
    public class NotificationService : INotificationService
    {
        public async Task SendTextToConfiguredContact()
        {
            var configuredPhoneNumber = Preferences.Default.Get(WatchMeConstants.PhoneNumberPreferencesKey, "");
            var message = "Andrew just started a WatchMe Routine. Click here to watch along: https://www.youtube.com/watch?v=dQw4w9WgXcQ";

            if (configuredPhoneNumber == null)
            {
                await ToastHelper.CreateToast(WatchMeConstants.Settings_PhoneNumber_NotConfigured);
                return;
            }
#if ANDROID
            if (!MauiProgram.ISEMULATED)
            {
                var smsM = SmsManager.Default;
                smsM.SendTextMessage(configuredPhoneNumber, null, message, null, null);
            }
#endif
        }
    }
}
