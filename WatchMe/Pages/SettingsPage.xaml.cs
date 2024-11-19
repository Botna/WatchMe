using WatchMe.Persistance;

namespace WatchMe.Pages;

public partial class SettingsPage : ContentPage
{
    public readonly ICloudProviderService _cloudProviderService;
    private string configuredConnectionString = string.Empty;
    public SettingsPage(ICloudProviderService cloudProviderService)
    {
        InitializeComponent();
        _cloudProviderService = cloudProviderService;
    }

    private async void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        configuredConnectionString = e.NewTextValue;
    }

    private async void OnSettingsPageSave(object sender, EventArgs e)
    {
        _cloudProviderService.SetAzureConnectionString(configuredConnectionString);
    }
}