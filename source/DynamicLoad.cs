using UnityEngine;

namespace SaveManager
{
    public class DynamicLoader : SingletonBehaviour<DynamicLoader>
    {
        public static new string AllowAutoCreate() => "[SM-DynamicLoader]";

        public void Load()
        {
            PauseMenuOverlay.Instance.message = "Currently requires a game restart.";
        }
    }
}
