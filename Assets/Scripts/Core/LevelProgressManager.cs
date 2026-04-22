using UnityEngine;

namespace ShadowClone.Core
{
    public static class LevelProgressManager
    {
        private const string HighestUnlockedLevelKey = "progress.highestUnlockedLevel";
        private const string StarsKeyPrefix = "progress.stars.";
        private const string BestScoreKeyPrefix = "progress.bestScore.";

        public const int FirstLevel = 1;
        public const int MaxLevel = 4;

        public static int GetHighestUnlockedLevel()
        {
            int highestUnlocked = PlayerPrefs.GetInt(HighestUnlockedLevelKey, FirstLevel);
            return Mathf.Clamp(highestUnlocked, FirstLevel, MaxLevel);
        }

        public static bool IsLevelUnlocked(int levelNumber)
        {
            if (levelNumber < FirstLevel || levelNumber > MaxLevel)
            {
                return false;
            }

            return levelNumber <= GetHighestUnlockedLevel();
        }

        public static void CompleteLevel(int levelNumber)
        {
            if (levelNumber < FirstLevel || levelNumber > MaxLevel)
            {
                return;
            }

            int currentHighest = GetHighestUnlockedLevel();
            int nextUnlocked = Mathf.Clamp(levelNumber + 1, FirstLevel, MaxLevel);
            int newHighest = Mathf.Max(currentHighest, nextUnlocked);

            if (newHighest != currentHighest)
            {
                PlayerPrefs.SetInt(HighestUnlockedLevelKey, newHighest);
            }

            PlayerPrefs.Save();
        }

        public static bool TryCompleteCurrentScene()
        {
            if (!TryGetLevelNumberForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, out int levelNumber))
            {
                return false;
            }

            CompleteLevel(levelNumber);
            return true;
        }

        public static int GetStars(int levelNumber)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(GetStarsKey(levelNumber), 0), 0, 3);
        }

        public static void SetStars(int levelNumber, int stars)
        {
            if (levelNumber < FirstLevel || levelNumber > MaxLevel)
            {
                return;
            }

            int clampedStars = Mathf.Clamp(stars, 0, 3);
            int currentStars = GetStars(levelNumber);
            if (clampedStars > currentStars)
            {
                PlayerPrefs.SetInt(GetStarsKey(levelNumber), clampedStars);
                PlayerPrefs.Save();
            }
        }

        public static int GetBestScore(int levelNumber)
        {
            return PlayerPrefs.GetInt(GetBestScoreKey(levelNumber), 0);
        }

        public static void SetBestScore(int levelNumber, int bestScore)
        {
            if (levelNumber < FirstLevel || levelNumber > MaxLevel)
            {
                return;
            }

            int currentScore = GetBestScore(levelNumber);
            if (bestScore > currentScore)
            {
                PlayerPrefs.SetInt(GetBestScoreKey(levelNumber), bestScore);
                PlayerPrefs.Save();
            }
        }

        public static bool TryGetLevelNumberForScene(string sceneName, out int levelNumber)
        {
            switch (sceneName)
            {
                case "Level1":
                case SceneRegistry.Tutorial:
                    levelNumber = 1;
                    return true;
                case "Level2":
                case SceneRegistry.ButtonDoor:
                    levelNumber = 2;
                    return true;
                case "Level3":
                case SceneRegistry.HazardTiming:
                    levelNumber = 3;
                    return true;
                case "Level4":
                case SceneRegistry.Final:
                    levelNumber = 4;
                    return true;
                default:
                    levelNumber = 0;
                    return false;
            }
        }

        public static string GetDefaultSceneNameForLevel(int levelNumber)
        {
            return levelNumber switch
            {
                1 => "Level1",
                2 => "Level2",
                3 => "Level3",
                4 => "Level4",
                _ => string.Empty
            };
        }

        public static string ResolvePlayableSceneName(int levelNumber, string preferredSceneName)
        {
            if (!string.IsNullOrWhiteSpace(preferredSceneName) && Application.CanStreamedLevelBeLoaded(preferredSceneName))
            {
                return preferredSceneName;
            }

            string alias = levelNumber switch
            {
                1 => SceneRegistry.Tutorial,
                2 => SceneRegistry.ButtonDoor,
                3 => SceneRegistry.HazardTiming,
                4 => SceneRegistry.Final,
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(alias) && Application.CanStreamedLevelBeLoaded(alias))
            {
                return alias;
            }

            return preferredSceneName;
        }

        private static string GetStarsKey(int levelNumber)
        {
            return $"{StarsKeyPrefix}{levelNumber}";
        }

        private static string GetBestScoreKey(int levelNumber)
        {
            return $"{BestScoreKeyPrefix}{levelNumber}";
        }
    }
}
