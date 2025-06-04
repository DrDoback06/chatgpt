using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;
    public Character character;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("CharacterManager instance assigned."); // Add this line to check if the instance is assigned
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
