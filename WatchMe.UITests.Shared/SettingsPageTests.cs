namespace UITests;

// This is an example of tests that do not need anything platform specific
public class SettingsPageTests : BaseTest
{
    [Test]
    public void SettignsPage_Loads()
    {
        var settingsButton = FindUIElement(AutomationConstants.Main_SettingsBtn);
        settingsButton.Click();
        Task.Delay(500).Wait(); // Wait for the click to register and show up on the screenshot

        App.GetScreenshot().SaveAsFile($"{nameof(SettignsPage_Loads)}.png");
    }

    [Test]
    public void SettingsPage_SavesAzureSCConnectionString()
    {
        var recordingButton = FindUIElement(AutomationConstants.Main_SettingsBtn);

        // Act
        recordingButton.Click();
        Task.Delay(500).Wait(); // Wait for the click to register and show up on the screenshot

        var entry = FindUIElement(AutomationConstants.Settings_AzureSCConnstry_Entry);
        entry.SendKeys("TestValue");

        var saveButton = FindUIElement(AutomationConstants.Settigs_SaveButton);
        saveButton.Click();
    }
}