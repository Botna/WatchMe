//namespace UITests;

//public class RecordingPageTests : BaseTest
//{
//    [Test]
//    public void RecordingPageTests_Loads()
//    {
//        var settingsButton = FindUIElement(AutomationConstants.Main_SettingsBtn);
//        settingsButton.Click();
//        Task.Delay(500).Wait(); // Wait for the click to register and show up on the screenshot

//        App.GetScreenshot().SaveAsFile($"{nameof(RecordingPageTests_Loads)}.png");
//    }

//    [Test]
//    public void SettingsPage_SavesAzureSCConnectionString()
//    {
//        var settingsButton = FindUIElement(AutomationConstants.Main_SettingsBtn);

//        // Act
//        settingsButton.Click();
//        Task.Delay(500).Wait(); // Wait for the click to register and show up on the screenshot

//        var entry = FindUIElement(AutomationConstants.Settings_AzureSCConnstry_Entry);
//        entry.SendKeys("TestValue");

//        var saveButton = FindUIElement(AutomationConstants.Settings_SaveButton);
//        saveButton.Click();
//    }
//}