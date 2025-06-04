using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAutoLoader : MonoBehaviour
{
    public string sceneToLoad = "CharacterCreation"; // Scene to load next

    void Start()
    {
        // Ensure managers are ready before loading next scene
        // Small delay might be needed in complex setups, but usually Start() order works okay here.
        Debug.Log($"AutoLoader: Loading scene '{sceneToLoad}'...");
        SceneManager.LoadScene(sceneToLoad);
    }
}