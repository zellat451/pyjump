using System.Text.Json;
using pyjump.Entities.Preferences;

namespace pyjump.Services
{
    public static class PreferenceService
    {
        private const string CheckboxPreferencesFileName = "checkbox_preferences.json";
        private static string CheckboxPreferencesFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", CheckboxPreferencesFileName);

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
    }
}
