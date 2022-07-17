using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using HarmonyLib;

using CommandTerminal;

namespace SaveManager
{
    class InitalLoadOverlay : SingletonBehaviour<InitalLoadOverlay>
    {
        public static new string AllowAutoCreate() => "[SM-InitialLoadOverlay]";

        public bool loadSelected = false;
        public bool overlayEnabled { get; private set; } = true;

        private readonly int windowID = GUIUtility.GetControlID(FocusType.Keyboard);

        private static float[] initialPosition = { Screen.width / 2, Screen.height / 2 };
        private static float[] windowSize = { Screen.width / 5, Screen.height / 2 };

        private Rect windowRect = new Rect(initialPosition[0] - windowSize[0] / 2, initialPosition[1] - windowSize[1] / 2, windowSize[0], windowSize[1]);

        private Vector2 saveScrollPosition = Vector2.zero;

        private List<string> saves { get => SaveManager.Instance.saveCache; }
        private int selectedSaveIndex = -1;
        public bool showBackups { get; private set; } = false;
        private string selection = "";
        private string message = "";

        private bool stylesCreated = false;

        private GUIStyle windowStyle;
        private GUIStyle titleStyle;
        private GUIStyle gridStyle;
        private GUIStyle selectionStyle;
        private GUIStyle toggleStyle;
        private GUIStyle messageStyle;

        protected override void Awake()
        {
            base.Awake();
            enabled = Main.enabled;
        }

        public void setupStyles()
        {
            windowStyle = new GUIStyle(GUI.skin.window) { };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                fontStyle = FontStyle.Bold,

                contentOffset = new Vector2(0, -2),
                padding = new RectOffset(5, 5, 5, 10),
            };

            gridStyle = new GUIStyle(GUI.skin.button)
            {
                contentOffset = new Vector2(0, -1),
                padding = new RectOffset(6, 6, 5, 8),
            };

            selectionStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleRight,

                contentOffset = new Vector2(0, -2),
                padding = new RectOffset(6, 6, 5, 8),
            };

            toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                alignment = TextAnchor.MiddleCenter,

                padding = new RectOffset(5, 5, 5, 5),
            };

            messageStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,

                padding = new RectOffset(5, 5, 5, 5),
            };

            stylesCreated = true;
        }

        public void enableOverlay()
        {
            selection = SaveManager.Instance.currentSaveName;
            message = "";
            overlayEnabled = true;
        }
        public void disableOverlay()
        {
            overlayEnabled = false;
        }

        public void OnGUI()
        {
            if (!overlayEnabled) return;
            if (!stylesCreated) setupStyles();

            GUI.backgroundColor = Color.black;
            windowRect = GUILayout.Window(windowID, windowRect, drawGUI, "", windowStyle);
        }

        private void drawGUI(int id)
        {
            GUILayout.Label("Save Manager", titleStyle);

            saveScrollPosition = GUILayout.BeginScrollView(saveScrollPosition, GUILayout.ExpandHeight(true));
            int clickedSaveIndex = GUILayout.SelectionGrid(selectedSaveIndex, saves.ToArray(), 1, gridStyle);
            if (selectedSaveIndex != clickedSaveIndex) indexButtonClicked(clickedSaveIndex);
            selectedSaveIndex = clickedSaveIndex;
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            selection = GUILayout.TextField(selection, selectionStyle, GUILayout.MaxWidth(180), GUILayout.Height(35));
            bool tempShowBackups = GUILayout.Toggle(showBackups, "Show Backups", toggleStyle, GUILayout.ExpandWidth(true));
            if (showBackups != tempShowBackups) showBackupsToggleClicked(tempShowBackups);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Load", GUILayout.ExpandWidth(true))) loadButtonClicked();

            GUILayout.Label(message, messageStyle, GUILayout.ExpandWidth(true));
        }

        private void indexButtonClicked(int index)
        {
            selection = saves[index];
        }
        private void showBackupsToggleClicked(bool show)
        {
            showBackups = show;
            SaveManager.Instance.refreshSaveCache(showBackups);
        }
        private void loadButtonClicked()
        {
            if (saves.Contains(selection))
            {
                disableOverlay();
                loadSelected = true;
                SaveManager.Instance.loadFromFile(selection);
            }
            else
            {
                message = "That save does not exist.";
            }
        }

        [HarmonyPatch(typeof(WorldStreamingInit), "Validate")]
        class InitialLoadPatch
        {
            public static bool Prefix(ref bool __result, WorldStreamingInit __instance)
            {
                if (Instance.loadSelected || Instance.saves.Count == 0)
                {
                    Instance.disableOverlay();
                    return true;
                }

                Instance.enableOverlay();
                __result = false;
                return false;
            }
        }
    }
}
