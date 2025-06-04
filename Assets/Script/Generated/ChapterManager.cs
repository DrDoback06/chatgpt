using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterManager : MonoBehaviour
{
    [Tooltip("List of chapter scene names in order")]
    public List<string> chapterScenes = new List<string>
    {
        "RuinedHomeTown",
        "HauntedForest",
        "RitualGrounds",
        "FinalTemple"
    };

    [Tooltip("Name of the player GameObject")] 
    public string playerObjectName = "Player";

    private int currentChapterIndex = -1;
    private GameObject player;

    private void Start()
    {
        player = GameObject.Find(playerObjectName);
        LoadNextChapter();
    }

    public void LoadNextChapter()
    {
        currentChapterIndex++;
        if (currentChapterIndex < chapterScenes.Count)
        {
            StartCoroutine(Transition(chapterScenes[currentChapterIndex]));
        }
        else
        {
            Debug.Log("ChapterManager: No more chapters to load.");
        }
    }

    private IEnumerator Transition(string sceneName)
    {
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);
        }

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        if (player != null)
        {
            player.transform.position = new Vector3(
                PlayerPrefs.GetFloat("PlayerX"),
                PlayerPrefs.GetFloat("PlayerY"),
                PlayerPrefs.GetFloat("PlayerZ"));
        }
    }
}
