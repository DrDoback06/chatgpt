using UnityEngine;
using System;

// Represents a single rolled affix instance on an item.
// Holds the type and the specific value rolled by the generator.
[System.Serializable]
public class ItemAffix
{
    [Tooltip("The type of stat modification this affix provides.")]
    public AffixType type; // Use the enum defined below or in a central location

    [Tooltip("The numerical value of this affix roll (e.g., +10 Strength, +15% Damage).")]
    public float value; // Use float for flexibility (percentages, flat values)

    // Reference to the definition that generated this affix (optional, for tooltip formatting)
    // We might not serialize this reference, just use type/value.
    // public AffixDefinition definition;

    // --- Enum for Affix Types ---
    // (Keep this updated with ALL possible stats/effects)
    public enum AffixType
    {
        // Attributes
        Strength,
        Agility,
        Intelligence,
        Vitality,
        AllAttributes,

        // Core Combat
        Damage_Flat,
        Damage_Percent,
        Armor_Flat,
        Armor_Percent,
        AttackSpeed_Percent,
        CriticalChance_Percent, // Renamed from CritChance_Percent
        CriticalDamage_Percent, // Renamed from CritDamage_Percent

        // Resources
        MaxHealth_Flat,
        MaxHealth_Percent,
        MaxMana_Flat,
        MaxMana_Percent,
        LifeOnHit,
        ManaOnHit,
        LifeSteal_Percent,
        ManaSteal_Percent,
        HealthRegen,
        ManaRegen,

        // Resistances
        FireResistance,
        ColdResistance,
        LightningResistance,
        PoisonResistance,
        AllResistances,

        // Utility
        MovementSpeed_Percent,
        MagicFind_Percent,
        GoldFind_Percent,
        LightRadius,

        // Skill System Related
        CooldownReduction_Percent,
        AreaOfEffect_Percent,
        ProjectileSpeed_Percent,
        ProjectileCount_Flat,
        ChainCount_Flat,
        PierceCount_Flat,
        BounceCount_Flat,
        ExplosionRadius_Percent,
        Duration_Percent,
        Knockback_Percent,

        // Status Effects
        StunChance_Percent,
        StunDuration_Percent,
        Slow_Percent,
        SlowDuration_Percent,
        BleedChance_Percent,
        BleedDamage_Percent,
        BleedDuration_Percent,
        PoisonChance_Percent,
        PoisonDamage_Percent,
        PoisonDuration_Percent,
        BurnChance_Percent,
        BurnDamage_Percent,
        BurnDuration_Percent,
        FreezeChance_Percent,
        FreezeDuration_Percent,
        ShockChance_Percent,
        ShockDamage_Percent,
        ShockDuration_Percent,
        CurseChance_Percent,
        CurseEffect_Percent,
        CurseDuration_Percent,
        BuffChance_Percent,
        BuffEffect_Percent,
        BuffDuration_Percent,
        DebuffChance_Percent,
        DebuffEffect_Percent,
        DebuffDuration_Percent,

        // On-Hit/On-Kill Effects
        HealOnHit_Flat,
        HealOnHit_Percent,
        ManaOnHit_Flat,
        ManaOnHit_Percent,
        HealOnKill_Flat,
        HealOnKill_Percent,
        ManaOnKill_Flat,
        ManaOnKill_Percent,

        // Experience/Currency
        ExperienceGain_Percent,
        SkillPointGain_Percent,
        AttributePointGain_Percent,
        TalentPointGain_Percent,

        // Progression/Penalty
        RespawnTimeReduction_Percent,
        DeathPenaltyReduction_Percent,

        // Item Meta
        ItemDurability_Percent,
        ItemValue_Percent,
        VendorPriceReduction_Percent,
        CraftingSuccess_Percent,
        EnchantingSuccess_Percent,

        // Socketing/Gems/Sets
        SocketCount_Flat,
        GemEffect_Percent,
        SetBonus_Percent,

        // Unique/Legendary/Etc.
        UniqueEffect_Percent,
        LegendaryEffect_Percent,
        MythicEffect_Percent,
        DivineEffect_Percent,

        // Skill Bonuses (More complex - e.g., "+1 To Fire Skills")
        PlusLevelToSkillID,
        PlusLevelToSkillTree,
        PlusLevelToClassSkills,

        // On Hit Effects (Chance to cast spell, apply debuff etc.)
        ChanceToCast_OnHit,
        ChanceToApplyDebuff_OnHit
        // Add many more...
    }

    // --- Constructor ---
    public ItemAffix(AffixType affixType, float affixValue)
    {
        this.type = affixType;
        this.value = affixValue;
    }

    // --- Tooltip Helper ---
    // Generates the display string (e.g., "+10 Strength", "+15% Fire Resistance")
    // Requires access to formatting rules, potentially from an AffixDefinition.
    public string GetAffixDescription()
    {
        string sign = value >= 0 ? "+" : "";
        string suffix = ""; // For percentages or other units

        // Determine suffix based on type (needs refinement)
        switch (type)
        {
            case AffixType.Damage_Percent:
            case AffixType.Armor_Percent:
            case AffixType.AttackSpeed_Percent:
            case AffixType.CriticalChance_Percent:
            case AffixType.CriticalDamage_Percent:
            case AffixType.MaxHealth_Percent:
            case AffixType.MaxMana_Percent:
            case AffixType.LifeSteal_Percent:
            case AffixType.ManaSteal_Percent:
            case AffixType.MovementSpeed_Percent:
            case AffixType.MagicFind_Percent:
            case AffixType.GoldFind_Percent:
                suffix = "%"; break;
        }

        // Format value (e.g., round floats)
        string valueStr = value.ToString("F0"); // Default to integer display
        if (suffix == "%") valueStr = (value * 100f).ToString("F0"); // Show percentages correctly *if* value is stored as 0.0-1.0

        return $"{sign}{valueStr}{suffix} {FormatAffixTypeName(type)}";
    }

    // Helper to make enum names readable (keep expanding this)
    private string FormatAffixTypeName(AffixType t)
    {
        switch(t) {
            case AffixType.Strength: return "Strength";
            case AffixType.Agility: return "Agility";
            case AffixType.Intelligence: return "Intelligence";
            case AffixType.Vitality: return "Vitality";
            case AffixType.AllAttributes: return "To All Attributes";
            case AffixType.LifeOnHit: return "Life Gained on Hit";
            case AffixType.ManaOnHit: return "Mana Gained on Hit";
            case AffixType.HealthRegen: return "Replenish Life";
            case AffixType.ManaRegen: return "Regenerate Mana";
            case AffixType.FireResistance: return "Fire Resistance";
            case AffixType.ColdResistance: return "Cold Resistance";
            case AffixType.LightningResistance: return "Lightning Resistance";
            case AffixType.PoisonResistance: return "Poison Resistance";
            case AffixType.AllResistances: return "To All Resistances";
            case AffixType.LightRadius: return "Light Radius";
            case AffixType.Damage_Flat: return "Damage";
            case AffixType.Damage_Percent: return "Enhanced Damage";
            case AffixType.Armor_Flat: return "Armor";
            case AffixType.Armor_Percent: return "Enhanced Defense";
            case AffixType.AttackSpeed_Percent: return "Increased Attack Speed";
            case AffixType.CriticalChance_Percent: return "Critical Strike Chance";
            case AffixType.CriticalDamage_Percent: return "Critical Strike Damage";
            case AffixType.MaxHealth_Flat: return "Maximum Life";
            case AffixType.MaxHealth_Percent: return "% Increased Maximum Life";
            case AffixType.MaxMana_Flat: return "Maximum Mana";
            case AffixType.MaxMana_Percent: return "% Increased Maximum Mana";
            case AffixType.LifeSteal_Percent: return "% Life Steal";
            case AffixType.ManaSteal_Percent: return "% Mana Steal";
            case AffixType.MovementSpeed_Percent: return "Faster Run/Walk";
            case AffixType.MagicFind_Percent: return "% Better Chance of Getting Magic Items";
            case AffixType.GoldFind_Percent: return "% Extra Gold From Monsters";
            case AffixType.CooldownReduction_Percent: return "% Reduced Cooldown";
            case AffixType.AreaOfEffect_Percent: return "% Increased Area of Effect";
            case AffixType.ProjectileSpeed_Percent: return "% Increased Projectile Speed";
            case AffixType.ProjectileCount_Flat: return "Projectile Count";
            case AffixType.ChainCount_Flat: return "Chain Count";
            case AffixType.PierceCount_Flat: return "Pierce Count";
            case AffixType.BounceCount_Flat: return "Bounce Count";
            case AffixType.ExplosionRadius_Percent: return "% Increased Explosion Radius";
            case AffixType.Duration_Percent: return "% Increased Duration";
            case AffixType.Knockback_Percent: return "% Increased Knockback";
            case AffixType.StunChance_Percent: return "% Stun Chance";
            case AffixType.StunDuration_Percent: return "% Stun Duration";
            case AffixType.Slow_Percent: return "% Slow";
            case AffixType.SlowDuration_Percent: return "% Slow Duration";
            case AffixType.BleedChance_Percent: return "% Bleed Chance";
            case AffixType.BleedDamage_Percent: return "% Bleed Damage";
            case AffixType.BleedDuration_Percent: return "% Bleed Duration";
            case AffixType.PoisonChance_Percent: return "% Poison Chance";
            case AffixType.PoisonDamage_Percent: return "% Poison Damage";
            case AffixType.PoisonDuration_Percent: return "% Poison Duration";
            case AffixType.BurnChance_Percent: return "% Burn Chance";
            case AffixType.BurnDamage_Percent: return "% Burn Damage";
            case AffixType.BurnDuration_Percent: return "% Burn Duration";
            case AffixType.FreezeChance_Percent: return "% Freeze Chance";
            case AffixType.FreezeDuration_Percent: return "% Freeze Duration";
            case AffixType.ShockChance_Percent: return "% Shock Chance";
            case AffixType.ShockDamage_Percent: return "% Shock Damage";
            case AffixType.ShockDuration_Percent: return "% Shock Duration";
            case AffixType.CurseChance_Percent: return "% Curse Chance";
            case AffixType.CurseEffect_Percent: return "% Curse Effect";
            case AffixType.CurseDuration_Percent: return "% Curse Duration";
            case AffixType.BuffChance_Percent: return "% Buff Chance";
            case AffixType.BuffEffect_Percent: return "% Buff Effect";
            case AffixType.BuffDuration_Percent: return "% Buff Duration";
            case AffixType.DebuffChance_Percent: return "% Debuff Chance";
            case AffixType.DebuffEffect_Percent: return "% Debuff Effect";
            case AffixType.DebuffDuration_Percent: return "% Debuff Duration";
            case AffixType.HealOnHit_Flat: return "Heal on Hit";
            case AffixType.HealOnHit_Percent: return "% Heal on Hit";
            case AffixType.ManaOnHit_Flat: return "Mana on Hit";
            case AffixType.ManaOnHit_Percent: return "% Mana on Hit";
            case AffixType.HealOnKill_Flat: return "Heal on Kill";
            case AffixType.HealOnKill_Percent: return "% Heal on Kill";
            case AffixType.ManaOnKill_Flat: return "Mana on Kill";
            case AffixType.ManaOnKill_Percent: return "% Mana on Kill";
            case AffixType.ItemDurability_Percent: return "% Item Durability";
            case AffixType.ItemValue_Percent: return "% Item Value";
            case AffixType.VendorPriceReduction_Percent: return "% Vendor Price Reduction";
            case AffixType.CraftingSuccess_Percent: return "% Crafting Success";
            case AffixType.EnchantingSuccess_Percent: return "% Enchanting Success";
            case AffixType.SocketCount_Flat: return "Socket Count";
            case AffixType.GemEffect_Percent: return "% Gem Effect";
            case AffixType.SetBonus_Percent: return "% Set Bonus";
            case AffixType.UniqueEffect_Percent: return "Unique Effect";
            case AffixType.LegendaryEffect_Percent: return "Legendary Effect";
            case AffixType.MythicEffect_Percent: return "Mythic Effect";
            case AffixType.DivineEffect_Percent: return "Divine Effect";
            case AffixType.PlusLevelToSkillID: return "Plus Level to Skill ID";
            case AffixType.PlusLevelToSkillTree: return "Plus Level to Skill Tree";
            case AffixType.PlusLevelToClassSkills: return "Plus Level to Class Skills";
            case AffixType.ChanceToCast_OnHit: return "Chance to Cast on Hit";
            case AffixType.ChanceToApplyDebuff_OnHit: return "Chance to Apply Debuff on Hit";
            default: return System.Text.RegularExpressions.Regex.Replace(t.ToString(), "([A-Z])", " $1").Trim(); // Add spaces
        }
    }
}