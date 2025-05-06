using System.Text.Json;
using pyjump.Entities.Preferences;

namespace pyjump.Services
{
    public static class PreferenceService
    {
        #region checkbox
        private const string CheckboxPreferencesFileName = "checkbox_preferences.json";
        private static string CheckboxPreferencesFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", CheckboxPreferencesFileName);

        public static void OverwriteCheckboxPreferences(CheckboxPreferences preferences)
        {
            var jsonString = JsonSerializer.Serialize(preferences);
            File.WriteAllText(CheckboxPreferencesFilePath, jsonString);

            // ping the checkboxes to update their state
            SingletonServices.MainForm.LoadCheckboxPreferences();
        }

        public static CheckboxPreferences GetCheckboxPreferences()
        {
            if (File.Exists(CheckboxPreferencesFilePath))
            {
                var json = File.ReadAllText(CheckboxPreferencesFilePath);
                var preferences = JsonSerializer.Deserialize<CheckboxPreferences>(json);
                if (preferences != null)
                {
                    return preferences;
                }
            }

            return [];
        }

        public static void SaveNewCheckboxPreferences(string name, bool value)
        {
            var preferences = new CheckboxPreferences();
            if (File.Exists(CheckboxPreferencesFilePath))
            {
                var json = File.ReadAllText(CheckboxPreferencesFilePath);
                preferences = JsonSerializer.Deserialize<CheckboxPreferences>(json);
            }
            preferences[name] = value;
            var jsonString = JsonSerializer.Serialize(preferences);
            File.WriteAllText(CheckboxPreferencesFilePath, jsonString);
        }
        #endregion

        #region others
        private const string OtherPreferencesFileName = "other_preferences.json";
        private static string OtherPreferencesFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", OtherPreferencesFileName);
        public static void OverwriteOtherPreferences(OtherPreferences preferences)
        {
            var jsonString = JsonSerializer.Serialize(preferences);
            File.WriteAllText(OtherPreferencesFilePath, jsonString);
            // ping the SingletonServices to update their state
            SingletonServices.SetPermissionFileLogging(preferences.AllowLogFile);
            SingletonServices.SetPermissionThreading(preferences.AllowThreading);
            SingletonServices.SetMaxThreads(preferences.MaxThreads);
            // ping the buttons & textboxes to update their state
            SingletonServices.MainForm.LoadOtherPreferences();
        }
        public static OtherPreferences GetOtherPreferences()
        {
            if (File.Exists(OtherPreferencesFilePath))
            {
                var json = File.ReadAllText(OtherPreferencesFilePath);
                var preferences = JsonSerializer.Deserialize<OtherPreferences>(json);
                if (preferences != null)
                {
                    return preferences;
                }
            }
            return new OtherPreferences
            {
                MaxThreads = 5,
                AllowThreading = true,
                AllowLogFile = false
            };
        }

        public static void SaveOtherPreferences()
        {
            var preferences = new OtherPreferences
            {
                MaxThreads = SingletonServices.MaxThreads,
                AllowThreading = SingletonServices.AllowThreading,
                AllowLogFile = SingletonServices.AllowLogFile
            };
            var jsonString = JsonSerializer.Serialize(preferences);
            File.WriteAllText(OtherPreferencesFilePath, jsonString);
        }
        #endregion
    }
}
