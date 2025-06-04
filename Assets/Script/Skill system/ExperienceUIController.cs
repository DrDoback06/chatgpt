using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceUIController : MonoBehaviour
{
    public Character character;
    public TMP_Text levelText;
    public Slider experienceSlider;

    private void Start()
    {
        // Initialize UI
        UpdateExperienceUI();

        // Subscribe to the character's OnExperienceChanged event
        character.OnExperienceChanged += UpdateExperienceUI;
    }

    // Commented out for future reference
    // public void SetLevel(int level)
    // {
    //     // Adjust enemy stats based on the level
    //     // For example:
    //     this.level = level;
    //     health = baseHealth + (level * healthPerLevel);
    //     damage = baseDamage + (level * damagePerLevel);
    //     experienceValue = baseExperience + (level * experiencePerLevel);
    // }

    public void UpdateExperienceUI()
    {
        levelText.text = "Level: " + character.Level;
        experienceSlider.maxValue = character.RequiredExperienceForNextLevel;
        experienceSlider.value = character.ExperiencePoints;
    }
}
