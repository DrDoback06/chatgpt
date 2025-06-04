using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillTreeUIInitializer : MonoBehaviour
{
    public SkillTreeUIController skillTreeUIController;
    public SkillDetailsToggle skillDetailsToggle;

    void Start()
    {
        if (CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
            skillTreeUIController.InitializeFromCharacter(CharacterManager.Instance.character);
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Town" && CharacterManager.Instance != null && CharacterManager.Instance.character != null)
        {
            skillTreeUIController.InitializeFromCharacter(CharacterManager.Instance.character);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
