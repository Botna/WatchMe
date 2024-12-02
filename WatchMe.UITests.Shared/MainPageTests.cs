using FluentAssertions;
using WatchMe.Config;
namespace UITests;

// This is an example of tests that do not need anything platform specific
public class MainPageTests : BaseTest
{
    [Test]
    public void AppLaunches()
    {
        App.GetScreenshot().SaveAsFile($"{nameof(AppLaunches)}.png");
    }

    [Test]
    public void ClickRecord_SettingsNotPresent_ToastsError()
    {
        var recordingButton = FindUIElement(AutomationConstants.Main_SettingsBtn);

        // Act
        recordingButton.Click();
        Task.Delay(500).Wait(); // Wait for the click to register and show up on the screenshot

        var toast = FindUIElementByXpath("/hierarchy/android.widget.Toast");

        var thing = toast.GetAttribute("text");
        thing.Should().Be(WatchMeConstants.Settings_ConnectionStringNotFound_AzureSC);
    }
}