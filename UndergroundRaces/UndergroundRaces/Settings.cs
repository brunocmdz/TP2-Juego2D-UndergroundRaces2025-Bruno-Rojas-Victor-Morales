using System;
namespace UndergroundRaces
{
    public static class Settings
    {
        public const int MaxLevel = 10;

        // Stored as 0..MaxLevel (user-facing 1..10, but we use 0..10 here and default to 5)
        public static int MusicLevel { get; set; } = 5;
        public static int SfxLevel { get; set; } = 5;
        public static int MasterLevel { get; set; } = 5;
        public static int BrightnessLevel { get; set; } = 10;

        public static float MusicVolume => Math.Clamp(MusicLevel / (float)MaxLevel, 0f, 1f);
        public static float SfxVolume => Math.Clamp(SfxLevel / (float)MaxLevel, 0f, 1f);
        public static float MasterVolume => Math.Clamp(MasterLevel / (float)MaxLevel, 0f, 1f);
        public static float Brightness => Math.Clamp(BrightnessLevel / (float)MaxLevel, 0f, 1f);

        // Events to notify listeners when a setting changes (useful to update audio instances)
        public static event Action OnMusicChanged;
        public static event Action OnSfxChanged;
        public static event Action OnMasterChanged;
        public static event Action OnBrightnessChanged;

        public static void IncreaseMusic() { if (MusicLevel < MaxLevel) { MusicLevel++; OnMusicChanged?.Invoke(); } }
        public static void DecreaseMusic() { if (MusicLevel > 0) { MusicLevel--; OnMusicChanged?.Invoke(); } }
        public static void IncreaseSfx() { if (SfxLevel < MaxLevel) { SfxLevel++; OnSfxChanged?.Invoke(); } }
        public static void DecreaseSfx() { if (SfxLevel > 0) { SfxLevel--; OnSfxChanged?.Invoke(); } }
        public static void IncreaseMaster() { if (MasterLevel < MaxLevel) { MasterLevel++; OnMasterChanged?.Invoke(); } }
        public static void DecreaseMaster() { if (MasterLevel > 0) { MasterLevel--; OnMasterChanged?.Invoke(); } }
        public static void IncreaseBrightness() { if (BrightnessLevel < MaxLevel) { BrightnessLevel++; OnBrightnessChanged?.Invoke(); } }
        public static void DecreaseBrightness() { if (BrightnessLevel > 0) { BrightnessLevel--; OnBrightnessChanged?.Invoke(); } }
    }
}
