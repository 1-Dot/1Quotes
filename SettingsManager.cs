using System;
using System.IO;
using System.Text.Json;

namespace _1Quotes;

internal enum InputTriggerMode
{
    ShiftBracket, // Shift + [ / Shift + ]
    BracketOnly   // 直接使用 [ / ]
}

internal class AppSettings
{
    public bool RunAtStartup { get; set; } = false;
    public InputTriggerMode TriggerMode { get; set; } = InputTriggerMode.ShiftBracket;
}

internal static class SettingsManager
{
    private static readonly string SettingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "1Quotes");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    public static AppSettings Current { get; private set; } = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var s = JsonSerializer.Deserialize<AppSettings>(json);
                if (s != null) Current = s;
            }
        }
        catch { }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}
