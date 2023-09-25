using HarmonyLib;

namespace ProfileCreateHotfix
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "neo.lbol.fixes.1.3.15splashScreen";
        public const string Name = "Intro screen fix";
        public const string version = "1.0.0";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
