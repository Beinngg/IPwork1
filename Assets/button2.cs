using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader2 : MonoBehaviour
{
    [Header("Main Menu Target")]
    [Tooltip("Scene name of your main menu (must be in Build Settings).")]
    [SerializeField] private string sceneName = "1 1";

    [Tooltip("Optional override. If >= 0, this build index will be used instead of sceneName.")]
    [SerializeField] private int sceneBuildIndex = -1;

    // Call this from your UI button OnClick
    public void LoadNewGame()
    {
        // Ensure the game isn't paused
        Time.timeScale = 1f;

        // For menus we want the mouse available
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // If an index is provided, prefer that
        if (sceneBuildIndex >= 0)
        {
            if (sceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(sceneBuildIndex);
            }
            else
            {
                Debug.LogError($"SceneLoader2: Build index {sceneBuildIndex} is not in Build Settings.");
            }
            return;
        }

        // Otherwise load by name
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader2: sceneName is empty. Set it in the Inspector.");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError(
                $"SceneLoader2: Scene '{sceneName}' not found in Build Settings. " +
                "Add it via File > Build Settings (Scenes In Build) or check the exact name."
            );
        }
    }
}
