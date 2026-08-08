using EpinelPS.Data;

namespace EpinelPS.Utils;

public class EquipmentUtils
{
    private const double OffensiveEffectGroupWeightMultiplier = 5.0;
    private const double OffensiveOptionValueWeightMultiplier = 2.0;

    private static readonly string[] OffensiveOptionKeywords =
    [
        "attack",
        "atk",
        "damage",
        "critical",
        "crit",
        "charge",
        "ammo",
        "reload",
        "rateoffire",
        "rate_of_fire",
        "accuracy",
        "hitrate",
        "hit_rate",
        "element",
        "weakpoint",
        "weak_point",
        "fullburst",
        "full_burst"
    ];

    /// <summary>
    /// Deducts materials from user's inventory and updates the response
    /// </summary>
    /// <param name="material">The material item to deduct</param>
    /// <param name="materialCost">Amount of material to deduct</param>
    /// <param name="user">The user whose inventory to update</param>
    /// <param name="responseItems">The response items list to update</param>
    /// <returns>True if deduction was successful, false otherwise</returns>
    public static bool DeductMaterials(DbItemData material, int materialCost, User user, IList<NetUserItemData> responseItems)
    {
        if (material.Count < materialCost)
            return false;

        material.Count -= materialCost;
        if (material.Count <= 0)
        {
            user.Items.Remove(material);
            NetUserItemData netItem = NetUtils.ToNet(material);
            netItem.Count = 0;
            responseItems.Add(netItem);
        }
        else
        {
            responseItems.Add(NetUtils.ToNet(material));
        }

        return true;
    }

    public static double ApplyOffensiveEffectGroupWeight(EquipmentOptionRecord option, double baseWeight)
    {
        return IsOffensiveOption(option)
            ? baseWeight * OffensiveEffectGroupWeightMultiplier
            : baseWeight;
    }

    public static long GetWeightedOptionRatio(EquipmentOptionRecord option)
    {
        long ratio = Math.Max(0, option.OptionRatio);
        if (IsOffensiveOption(option))
        {
            ratio = (long)Math.Ceiling(ratio * OffensiveOptionValueWeightMultiplier);
        }

        return Math.Max(1, ratio);
    }

    public static int SelectStateEffectId(EquipmentOptionRecord option, Random random)
    {
        if (option.StateEffectList == null || option.StateEffectList.Count == 0)
        {
            throw new InvalidOperationException($"StateEffectList is null or empty for option {option.Id}");
        }

        if (!IsOffensiveOption(option))
        {
            int randomIndex = random.Next(option.StateEffectList.Count);
            return option.StateEffectList[randomIndex].StateEffectId;
        }

        long totalWeight = option.StateEffectList.Sum(x => GetStateEffectLevelWeight(x));
        long randomValue = random.NextInt64(0, totalWeight);
        long cumulativeWeight = 0;

        foreach (StateEffectList stateEffect in option.StateEffectList.OrderBy(x => x.StateEffectLevel))
        {
            cumulativeWeight += GetStateEffectLevelWeight(stateEffect);
            if (randomValue < cumulativeWeight)
            {
                return stateEffect.StateEffectId;
            }
        }

        return option.StateEffectList
            .OrderByDescending(x => x.StateEffectLevel)
            .First()
            .StateEffectId;
    }

    public static bool IsOffensiveOption(EquipmentOptionRecord option)
    {
        string key = NormalizeOptionKey(option.DescriptionLocalkey);

        return OffensiveOptionKeywords.Any(key.Contains);
    }

    private static string NormalizeOptionKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key)
            ? string.Empty
            : key.ToLowerInvariant();
    }

    private static long GetStateEffectLevelWeight(StateEffectList stateEffect)
    {
        int level = Math.Max(1, stateEffect.StateEffectLevel);
        return (long)level * level;
    }
}
