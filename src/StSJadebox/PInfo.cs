using HarmonyLib;

namespace StSJadebox
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "neo.lbol.gameplay.StSJadeBox";
        public const string Name = "StS Jade Box(es)";
        public const string version = "0.5.0";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
