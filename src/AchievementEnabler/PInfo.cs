using HarmonyLib;

namespace AchievementEnabler
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "neo.lbol.qol.achievementEnabler";
        public const string Name = "Achievement Enabler";
        public const string version = "1.1.1";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
