using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlashbackManager : MonoBehaviour
{
    [Tooltip("Name of the player GameObject")]
    public string playerObjectName = "Player";

    private string previousScene;
    private GameObject player;
    private Vector3 savedPosition;
    private Character savedCharacter;

    public void StartFlashback(string flashbackScene, Character flashbackCharacter, Vector3 startPosition)
    {
        StartCoroutine(FlashbackRoutine(flashbackScene, flashbackCharacter, startPosition));
    }

    private IEnumerator FlashbackRoutine(string flashbackScene, Character flashbackCharacter, Vector3 startPosition)
    {
        player = GameObject.Find(playerObjectName);
        if (player != null)
        {
            savedPosition = player.transform.position;
        }
        previousScene = SceneManager.GetActiveScene().name;
        savedCharacter = CharacterManager.Instance.character;
        CharacterManager.Instance.character = flashbackCharacter;

        yield return SceneManager.LoadSceneAsync(flashbackScene);

        if (player != null)
        {
            player = GameObject.Find(playerObjectName);
            player.transform.position = startPosition;
        }
    }

    public void EndFlashback()
    {
        StartCoroutine(EndFlashbackRoutine());
    }

    private IEnumerator EndFlashbackRoutine()
    {
        yield return SceneManager.LoadSceneAsync(previousScene);
        CharacterManager.Instance.character = savedCharacter;
        if (player == null)
        {
            player = GameObject.Find(playerObjectName);
        }
        if (player != null)
        {
            player.transform.position = savedPosition;
        }
    }
}
