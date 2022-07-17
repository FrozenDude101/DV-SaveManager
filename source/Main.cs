using System.Reflection;

using UnityEngine;

using UnityModManagerNet;
using HarmonyLib;

namespace SaveManager
{
    public class Main
    {
        public static UnityModManager.ModEntry mod;
        public static Harmony harmony;

        public static bool enabled;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            enabled = true;

            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }
    }
}