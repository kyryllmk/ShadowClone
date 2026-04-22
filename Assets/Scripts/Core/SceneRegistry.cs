namespace ShadowClone.Core
{
    public static class SceneRegistry
    {
        public const string MainMenu = "MainMenu";
        public const string LevelSelect = "LevelSelect";
        public const string Tutorial = "Level_01_Tutorial";
        public const string ButtonDoor = "Level_02_ButtonDoor";
        public const string HazardTiming = "Level_03_HazardTiming";
        public const string Final = "Level_04_Final";
        public const string Prototype = "Gameplay_Prototype";

        public static readonly string[] CampaignLevels =
        {
            Tutorial,
            ButtonDoor,
            HazardTiming,
            Final
        };

        public static bool TryGetNextCampaignScene(string currentSceneName, out string nextSceneName)
        {
            nextSceneName = null;

            for (int i = 0; i < CampaignLevels.Length; i++)
            {
                if (CampaignLevels[i] != currentSceneName)
                {
                    continue;
                }

                int nextIndex = i + 1;
                if (nextIndex >= CampaignLevels.Length)
                {
                    return false;
                }

                nextSceneName = CampaignLevels[nextIndex];
                return true;
            }

            return false;
        }
    }
}
