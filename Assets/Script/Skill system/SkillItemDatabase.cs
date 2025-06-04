using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[CreateAssetMenu(fileName = "SkillIconDatabase", menuName = "ScriptableObjects/SkillIconDatabase", order = 1)]
public class SkillIconDatabase : ScriptableObject
{
    [System.Serializable]
    public class SkillIconEntry
    {
        public string characterClassName;
        public string skillName;
        public Sprite skillIcon;
    }

    public List<SkillIconEntry> skillIconEntries;

    public Sprite GetSkillIcon(string characterClassName, string skillName)
    {
        return skillIconEntries.FirstOrDefault(entry => entry.characterClassName == characterClassName && entry.skillName == skillName)?.skillIcon;
    }
}
