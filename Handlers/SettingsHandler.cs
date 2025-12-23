using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ADMP.code
{
    public class SettingsHandler
    {
        public Settings AppSettings { get; set; } = new();

        public void LoadSettings()
        {
            Debug.WriteLine("Loading settings...");

            if (!File.Exists("appsettings.json"))
            {
                Debug.WriteLine("File not found/not accessible!");
                return;
            }

            string jsonText = File.ReadAllText("appsettings.json");

            if (string.IsNullOrEmpty(jsonText))
            {
                Debug.WriteLine("Settings file is empty/corrupted!");
                return;
            }
            try
            {
                AppSettings = JsonSerializer.Deserialize<Settings>(jsonText)!;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error deserializing settings: " + ex.Message);
                return;
            }
            //Debug.WriteLine("loaded: " + jsonText);
            //Debug.WriteLine($"Found: {appSettings.LastOpened.Count} recent files");
        }

        public void SaveSettings()
        {
            Debug.WriteLine("Saving settings...");
            string settingsJson = JsonSerializer.Serialize<Settings>(AppSettings);
            Debug.WriteLine("Serialized settings: " + settingsJson);
            File.WriteAllText("appsettings.json", settingsJson);
        }
    }

    public class Settings
    {
        public Queue<String> LastOpened { get; set; }//would be better to just have a getter, but json serializer pretty much needs a setter
        public int? VolumeLevel { get; set; }

        public Settings()
        {
            LastOpened = new Queue<string>();
            VolumeLevel = null;
        }
        public Settings(Queue<string> lastOpened, int volume, bool isMuted)
        {
            LastOpened = lastOpened;
            VolumeLevel = volume;
        }

        public void AddRecent(string filePath)
        {
            if (LastOpened.Count >= 5)
            {
                LastOpened.Dequeue();
            }
            LastOpened.Enqueue(filePath);
        }
    }
}
