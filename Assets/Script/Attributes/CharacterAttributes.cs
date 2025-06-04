using System;
using UnityEngine; // Added for Tooltip if we make fields visible later

// Defines the core attributes for a character.
[System.Serializable]
public class CharacterAttributes
{
    // Tooltip examples if you expose these directly later, or for reference
    // [Tooltip("Influences physical damage and carrying capacity.")]
    [SerializeField] private int strength;
    // [Tooltip("Influences attack speed, critical hit chance, and evasion.")]
    [SerializeField] private int agility;
    // [Tooltip("Influences magical damage, mana pool, and skill effectiveness.")]
    [SerializeField] private int intelligence;
    // [Tooltip("Influences health pool and resistances.")]
    [SerializeField] private int vitality;

    // Event triggered when any attribute value changes. UI and other systems can subscribe to this.
    public event Action OnAttributesChanged;

    // --- Public Properties for Access ---

    public int Strength
    {
        get { return strength; }
        // Make setter private or protected if only ChangeAttribute should modify it
        set
        {
            if (strength != value)
            {
                strength = Mathf.Max(0, value); // Prevent negative attributes
                OnAttributesChanged?.Invoke(); // Invoke event on change
            }
        }
    }

    public int Agility
    {
        get { return agility; }
        set
        {
            if (agility != value)
            {
                agility = Mathf.Max(0, value);
                OnAttributesChanged?.Invoke();
            }
        }
    }

    public int Intelligence
    {
        get { return intelligence; }
        set
        {
            if (intelligence != value)
            {
                intelligence = Mathf.Max(0, value);
                OnAttributesChanged?.Invoke();
            }
        }
    }

    public int Vitality
    {
        get { return vitality; }
        set
        {
            if (vitality != value)
            {
                vitality = Mathf.Max(0, value);
                OnAttributesChanged?.Invoke();
            }
        }
    }

    // --- Constructor ---
    public CharacterAttributes(int strength, int agility, int intelligence, int vitality)
    {
        // Use properties in constructor to ensure validation and event trigger (if needed, though less common here)
        this.strength = Mathf.Max(0, strength);
        this.agility = Mathf.Max(0, agility);
        this.intelligence = Mathf.Max(0, intelligence);
        this.vitality = Mathf.Max(0, vitality);
        // No event invocation needed on initial creation usually
    }

    // --- Methods ---

    /// <summary>
    /// Changes a specific attribute by a delta amount.
    /// Invokes the OnAttributesChanged event.
    /// Use this for temporary buffs/debuffs or direct modifications.
    /// </summary>
    /// <param name="attribute">The attribute to change.</param>
    /// <param name="delta">The amount to add (can be negative).</param>
    public void ChangeAttribute(CharacterAttributesType attribute, int delta)
    {
        switch (attribute)
        {
            case CharacterAttributesType.Strength:
                Strength += delta; // Uses property setter
                break;
            case CharacterAttributesType.Agility:
                Agility += delta;
                break;
            case CharacterAttributesType.Intelligence:
                Intelligence += delta;
                break;
            case CharacterAttributesType.Vitality:
                Vitality += delta;
                break;
            default:
                Debug.LogWarning($"ChangeAttribute called with invalid type: {attribute}");
                break;
        }
        // Event is invoked by property setters now, so no need to invoke here explicitly
        // if (changed) {
        //     OnAttributesChanged?.Invoke(); // Let other systems know something changed
        // }
    }
}

// We still need the enum defined somewhere accessible, like outside or in its own file.
// Make sure this enum definition exists.
public enum CharacterAttributesType
{
    Strength,
    Agility,
    Intelligence,
    Vitality
}