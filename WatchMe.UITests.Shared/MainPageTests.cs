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
        var recordingButton = FindUIElement("Main_RecordingStartBtn");

        // Act
        recordingButton.Click();
        Task.Delay(500).Wait(); // Wait for the click to register and show up on the screenshot

        var toast = FindUIElementByXpath("/hierarchy/android.widget.Toast");

        var thing = toast.GetAttribute("text");
        thing.Should().Be(WatchMeConstants.SETTINGS_CONNECTIONSTRINGNOTFOUND_AZURE);
        //thing.Should
        // Assert
        //var webDriver = new WebDriver()
        //WebDriverWait waitForToast = new WebDriverWait(;

        //waitForToast.Until(ExpectedConditions.presenceOfElementLoacted(By.xpath("/hierarchy/android.widget.Toast")));

        //String toastMessage = driver.findElement((By.xpath("/hierarchy/android.widget.Toast")).getText();

        //System.out.println(toastMessage);

    }
}