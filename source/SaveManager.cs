using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using HarmonyLib;

namespace SaveManager
{
    class SaveManager : SingletonBehaviour<SaveManager>
    {
        public static new string AllowAutoCreate() => "[SM-SaveManager]";

        public List<string> saveCache { get; private set; }

        public string saveDirectoryPath;

        public string currentSaveName = "savegame";

        protected override void Awake()
        {
            base.Awake();
            enabled = Main.enabled;

            string saveName = SingletonBehaviour<SaveGameManager>.Instance.GetSavePath();
            saveDirectoryPath = saveName.Substring(0, saveName.Length - "savegame".Length);

            refreshSaveCache(false);
        }

        public void refreshSaveCache(bool backups = false)
        {
            saveCache = getValidSaves(backups);
        }

        public List<string> getValidSaves(bool backups = false)
        {
            List<string> allFiles = new List<string>(Directory.GetFiles(saveDirectoryPath));
            List<string> filteredFiles = new List<string>();

            for (int i = 0; i < allFiles.Count; i++)
            {
                string file = allFiles[i];
                file = file.Substring(saveDirectoryPath.Length);

                if (file.EndsWith("json")) continue;
                if (file.EndsWith("ini")) continue;
                if (!backups & (file.EndsWith("bak") || file.Contains("_backup_") || file.Contains("_old_version"))) continue;

                filteredFiles.Add(file);
            }

            return filteredFiles;
        }

        public void loadFromFile(string name)
        {
            currentSaveName = name;
            StartCoroutine(
                (IEnumerator)typeof(WorldStreamingInit)
                    .GetMethod("LoadingRoutine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(SingletonBehaviour<WorldStreamingInit>.Instance, null)
            );
        }
        public void saveToFile(string name)
        {
            currentSaveName = name;
            SingletonBehaviour<SaveGameManager>.Instance.Save();
        }
        
        public void copySaveFile(string target, string name)
        {
            File.Copy(saveDirectoryPath + target, saveDirectoryPath + name);
        }
        public void deleteSaveFile(string name)
        {
            File.Delete(saveDirectoryPath + name);
        }

        [HarmonyPatch(typeof(SaveGameManager), "GetSavePath")]
        public class SaveFilePathPatch
        {
            public static void Postfix(ref string __result)
            {
                bool encrypted = SingletonBehaviour<SaveGameManager>.Instance.useEncryption;
                __result = __result.Substring(0, __result.Length - (encrypted ? "savegame" : "savegame.json").Length);
                __result += Instance.currentSaveName;
            }

        }
    }
}
