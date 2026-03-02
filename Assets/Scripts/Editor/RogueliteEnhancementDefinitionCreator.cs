using UnityEngine;
using UnityEditor;

namespace BladeParry.Editor
{
    /// <summary>
    /// Creates RogueliteEnhancementDefinition assets for all enhancements from the reference spreadsheet.
    /// Menu: Tools > BladeParry > Create Enhancement Definitions.
    /// Add the created assets to RogueliteProgressionConfig.EnhancementPool and add keys to the Enhancements localization table (Id_Name, Id_Desc).
    /// </summary>
    public static class RogueliteEnhancementDefinitionCreator
    {
        private const string DefaultOutputPath = "Assets/Data/RogueliteEnhancements";

        private struct EnhancementEntry
        {
            public string Id;
            public RogueliteEnhancementEffectType EffectType;
            public float BaseValue;
            public int MaxLevel;
        }

        [MenuItem("Tools/BladeParry/Create Enhancement Definitions")]
        public static void CreateAll()
        {
            var entries = new[]
            {
                new EnhancementEntry { Id = "Gambeson", EffectType = RogueliteEnhancementEffectType.IgnoreFirstDamagePerCombo, BaseValue = 1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "SerpentsCoil", EffectType = RogueliteEnhancementEffectType.ChanceIgnoreEachHit, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "PhoenixFeather", EffectType = RogueliteEnhancementEffectType.RegenOnFullPerfectParryCombo, BaseValue = 0.5f, MaxLevel = 3 },
                new EnhancementEntry { Id = "Whetstone", EffectType = RogueliteEnhancementEffectType.DamageBonus, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "SurgeonsEdge", EffectType = RogueliteEnhancementEffectType.DamageBonusPerfectParryCombo, BaseValue = 0.15f, MaxLevel = 3 },
                new EnhancementEntry { Id = "StaggeringCloak", EffectType = RogueliteEnhancementEffectType.ChanceDamageEndsCombo, BaseValue = 0.15f, MaxLevel = 3 },
                new EnhancementEntry { Id = "VitalEssence", EffectType = RogueliteEnhancementEffectType.Heal, BaseValue = 1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "HeartOfTheBull", EffectType = RogueliteEnhancementEffectType.MaxHealthBonus, BaseValue = 1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "RespiteCharm", EffectType = RogueliteEnhancementEffectType.PerfectParryIncreasesPauseBeforeNextCombo, BaseValue = 0.5f, MaxLevel = 3 },
                new EnhancementEntry { Id = "WatchersEye", EffectType = RogueliteEnhancementEffectType.PerfectParryIncreasesNextWindUp, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "SoulAnchor", EffectType = RogueliteEnhancementEffectType.ReviveAtEndOfComboWithXLives, BaseValue = 1f, MaxLevel = 2 },
                new EnhancementEntry { Id = "CrescendoBlade", EffectType = RogueliteEnhancementEffectType.DamageScalesWithCombo, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "VeteransCarapace", EffectType = RogueliteEnhancementEffectType.DamageReceivedDecreasesWithCombo, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "CadenceStone", EffectType = RogueliteEnhancementEffectType.DamageEveryXPerfectParries, BaseValue = 1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "TrinitySeal", EffectType = RogueliteEnhancementEffectType.AfterThreePerfectParriesNextFullParryBonusDamage, BaseValue = 1f, MaxLevel = 2 },
                new EnhancementEntry { Id = "BurdenStone", EffectType = RogueliteEnhancementEffectType.EnemySlowsWithCombo, BaseValue = 0.05f, MaxLevel = 3 },
                new EnhancementEntry { Id = "TelegraphBell", EffectType = RogueliteEnhancementEffectType.LongerWindUp, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "GracePeriodRing", EffectType = RogueliteEnhancementEffectType.LongerWindDown, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "SeveranceToken", EffectType = RogueliteEnhancementEffectType.ChancePerfectParryMoreCombo, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "ExecutionersMark", EffectType = RogueliteEnhancementEffectType.DamageInverseToRemainingHealth, BaseValue = 0.2f, MaxLevel = 3 },
                new EnhancementEntry { Id = "GuardiansFavor", EffectType = RogueliteEnhancementEffectType.ChanceAutoParryOnFail, BaseValue = 0.1f, MaxLevel = 2 },
                new EnhancementEntry { Id = "FrayCharm", EffectType = RogueliteEnhancementEffectType.ChancePerfectParryReduceComboCount, BaseValue = 0.15f, MaxLevel = 3 },
                new EnhancementEntry { Id = "EnduranceWard", EffectType = RogueliteEnhancementEffectType.ShieldWhenComboExceedsN, BaseValue = 1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "SwordGuard", EffectType = RogueliteEnhancementEffectType.ChancePerfectParryNextAttackFromGivenDirection, BaseValue = 0.2f, MaxLevel = 2 },
                new EnhancementEntry { Id = "Oil", EffectType = RogueliteEnhancementEffectType.ReceiveMoreDealMore, BaseValue = 0.15f, MaxLevel = 2 },
                new EnhancementEntry { Id = "BetterCuttingEdge", EffectType = RogueliteEnhancementEffectType.DamageBonusBasicParry, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "BetterGuard", EffectType = RogueliteEnhancementEffectType.PerfectParryRatioBonus, BaseValue = 0.1f, MaxLevel = 3 },
                new EnhancementEntry { Id = "BetterTip", EffectType = RogueliteEnhancementEffectType.DamageBonusFullComboParry, BaseValue = 0.2f, MaxLevel = 3 },
            };

            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/RogueliteEnhancements"))
                AssetDatabase.CreateFolder("Assets/Data", "RogueliteEnhancements");

            int created = 0;
            foreach (var e in entries)
            {
                string path = $"{DefaultOutputPath}/{e.Id}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<RogueliteEnhancementDefinition>(path);
                if (existing != null)
                {
                    var so = new SerializedObject(existing);
                    so.FindProperty("id").stringValue = e.Id;
                    so.FindProperty("baseValue").floatValue = e.BaseValue;
                    so.FindProperty("maxLevel").intValue = e.MaxLevel;
                    so.FindProperty("effectType").enumValueIndex = (int)e.EffectType;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(existing);
                    continue;
                }

                var asset = ScriptableObject.CreateInstance<RogueliteEnhancementDefinition>();
                var serialized = new SerializedObject(asset);
                serialized.FindProperty("id").stringValue = e.Id;
                serialized.FindProperty("baseValue").floatValue = e.BaseValue;
                serialized.FindProperty("maxLevel").intValue = e.MaxLevel;
                serialized.FindProperty("effectType").enumValueIndex = (int)e.EffectType;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.CreateAsset(asset, path);
                created++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"RogueliteEnhancementDefinitionCreator: Created {created} new assets, updated existing. Add them to RogueliteProgressionConfig.EnhancementPool and add keys (Id_Name, Id_Desc) to the Enhancements localization table.");
        }
    }
}
