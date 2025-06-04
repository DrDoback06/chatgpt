using UnityEngine;
using TMPro;

public class SkillDetailsToggle : MonoBehaviour
{
    public GameObject skillDetailsPanel;
    public TMP_Text skillNameText;
    public TMP_Text skillDescriptionText;
    public TMP_Text skillTypeText;
    public TMP_Text requiredLevelText;
    public TMP_Text cooldownTimeText;

    public void ShowSkillDetails(Skill skill)
    {
        Debug.Log("Showing skill details: " + skill.name); // Add this line

        skillNameText.text = skill.name;
        skillDescriptionText.text = skill.description;
        skillTypeText.text = skill.skillType.ToString();
        requiredLevelText.text = "Required Level: " + skill.requiredLevel;
        cooldownTimeText.text = "Cooldown: " + skill.cooldown + "s";
        skillDetailsPanel.SetActive(true);
    }

    public void HideSkillDetails()
    {
        Debug.Log("Hiding skill details"); // Add this line

        skillDetailsPanel.SetActive(false);
    }


    private void Start()
    {
        // Set skill details panel to not visible by default
        skillDetailsPanel.SetActive(false);
    }
}
