using System;
using System.Collections.Generic;
using System.IO;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Stats
{
    public class Stats
    {
        // Derived from AttackableUnit
        private float _currentHealth;
        public float Level1Health { get; set; }
        public float HealthGrowth { get; set; }
        private float _flatHealthBonus;
        private float _percentHealthBonus;
        public float CurrentHealth
        {
            get => _currentHealth.WithHardCaps(0, TotalHealth);
            set => _currentHealth = value;
        }
        public float FlatHealthBonus
        {
            get => _flatHealthBonus;
            set
            {
                if (value == _flatHealthBonus)
                {
                    return;
                }

                var percentHp = CurrentHealthPercent;
                _flatHealthBonus = value;
                CurrentHealth = TotalHealth * percentHp;
            }
        }
        public float PercentHealthBonus
        {
            get => _percentHealthBonus;
            set
            {
                if (value == _percentHealthBonus)
                {
                    return;
                }

                var percentHp = CurrentHealthPercent;
                _percentHealthBonus = value;
                CurrentHealth = TotalHealth * percentHp;
            }
        }

        public float BaseHealth => Level1Health + (Level - 1) * HealthGrowth;
        public float TotalHealth => (BaseHealth + FlatHealthBonus) * (1 + PercentHealthBonus);
        public float CurrentHealthPercent => CurrentHealth / TotalHealth;

        private float _currentPar;
        public float Level1Par { get; set; }
        public float ParGrowth { get; set; }
        private float _flatParBonus;
        private float _percentParBonus;
        public float CurrentPar
        {
            get => _currentPar.WithHardCaps(0, TotalPar);
            set => _currentPar = value;
        }
        public float FlatParBonus
        {
            get => _flatParBonus;
            set
            {
                if (value == _flatParBonus)
                {
                    return;
                }

                var percentHp = CurrentParPercent;
                _flatParBonus = value;
                CurrentPar = TotalPar * percentHp;
            }
        }
        public float PercentParBonus
        {
            get => _percentParBonus;
            set
            {
                if (value == _percentParBonus)
                {
                    return;
                }

                var percentHp = CurrentParPercent;
                _percentParBonus = value;
                CurrentPar = TotalPar * percentHp;
            }
        }

        public float BasePar => Level1Par + (Level - 1) * ParGrowth;
        public float TotalPar => (BasePar + FlatParBonus) * (1 + PercentParBonus);
        public float CurrentParPercent => CurrentPar / TotalPar;

        public bool IsInvulnerable { get; set; }
        public bool IsPhysicalImmune { get; set; }
        public bool IsMagicImmune { get; set; }
        public bool IsTargetable { get; set; } = true;
        public bool IsLifestealImmune { get; set; }
        public IsTargetableToTeamFlags IsTargetableToTeam { get; set; } = (IsTargetableToTeamFlags)0x2000000;
        
        // Derived from ObjAIBase
        public ActionState ActionState { get; set; } = (ActionState)0x800007;
        public float LifeTime { get; set; }
        public float MaxLifeTime { get; set; }
        public float LifeTimeTicks { get; set; }

        public float PhysicalDamageReductionPercent { get; set; }
        public float MagicalDamageReductionPercent { get; set; }

        // Derived from Champion
        public float Gold { get; set; }
        public float TotalGold { get; set; }
        public uint SpellEnabledBitFieldLower1 { get; set; } = uint.MaxValue;
        public uint SpellEnabledBitFieldUpper1 { get; set; } = uint.MaxValue;
        public uint SpellEnabledBitFieldLower2 { get; set; } = uint.MaxValue;
        public uint SpellEnabledBitFieldUpper2 { get; set; } = uint.MaxValue;
        public uint EvolvePoints { get; set; }
        public uint EvolveFlags { get; set; }
        public float[] ManaCost { get; set; } = new float[4];
        public float[] ManaCostEx { get; set; } = new float[16];
        public uint Level { get; set; } = 1;
        public uint NumberOfNeutralMinionsKilled { get; set; }
        public float PassiveCooldownEndTime { get; set; }
        public float PassiveCooldownTotalTime { get; set; }
        public float Experience { get; set; }
        public float PercentSpellCostReduction { get; set; }

        // General stat data

        // Movement Speed
        public float PercentMovementSpeedBonus { get; private set; } = 1;
        public float SlowResistPercent { get; set; }
        public List<float> SlowsApplied { get; set; } = new List<float>();
        public float BaseMovementSpeed { get; set; }
        public float FlatMovementSpeedBonus { get; set; }
        public float TotalMovementSpeed
        {
            get
            {
                var total = (BaseMovementSpeed + FlatMovementSpeedBonus) * PercentMovementSpeedBonus;

                var slowRatio = 0f;
                if (SlowsApplied.Count > 0)
                {
                    SlowsApplied.Sort();
                    SlowsApplied.Reverse();
                    slowRatio = SlowsApplied[0];
                    for (var i = 1; i < SlowsApplied.Count; i++)
                    {
                        slowRatio *= 1 - SlowsApplied[i] * 0.35f;
                    }
                }

                total *= 1 - slowRatio * SlowResistPercent;

                // soft caps
                if (total > 490)
                {
                    total = total * 0.5f + 230;
                }
                else if (total > 415)
                {
                    total = total * 0.8f + 83;
                }
                else if (total < 220)
                {
                    total = total * 0.5f + 110;
                }

                return Math.Max(0, total);
            }
        }

        public void AddPercentMovementSpeedBonus(float percent)
        {
            PercentMovementSpeedBonus *= 1 + percent;
        }

        public void RemovePercentMovementSpeedBonus(float percent)
        {
            PercentMovementSpeedBonus /= 1 + percent;
        }

        // Attack Speed
        public float AttackDelay { get; set; }
        public float BaseAttackSpeed => 0.625f / (1 + AttackDelay);
        public float PercentAttackSpeedMod { get; set; }
        public float AttackSpeedGrowth { get; set; }
        public float PercentAttackSpeedDebuff { get; private set; } = 1;
        public float TotalAttackSpeed
        {
            get
            {
                var total = BaseAttackSpeed * (1 + (Level - 1) * AttackSpeedGrowth + PercentAttackSpeedMod)
                                            * PercentAttackSpeedDebuff;
                if (total < 0.2f)
                {
                    return 0.2f;
                }

                return Math.Min(total, 2.5f);
            }
        }

        public void AddAttackSpeedDebuff(float percent)
        {
            PercentAttackSpeedDebuff *= 1 - percent;
        }

        public void RemoveAttackSpeedDebuff(float percent)
        {
            PercentAttackSpeedDebuff /= 1 - percent;
        }

        // Attack Range
        public float BaseAttackRange { get; set; }
        public float FlatAttackRangeMod { get; set; }
        public float PercentAttackRangeMod { get; set; }
        public float FlatAttackRangeDebuff { get; set; }
        public float PercentAttackRangeDebuff { get; set; }
        public float TotalAttackRange
        {
            get
            {
                var total = BaseAttackRange + FlatAttackRangeMod;
                total = total * 1 + PercentAttackRangeMod - FlatAttackRangeDebuff;
                return Math.Max(0, total * 1 - PercentAttackRangeDebuff);
            }
        }

        // Critical Chance
        public float FlatCriticalChanceMod { get; set; }
        public float CriticalChance
        {
            get => FlatCriticalChanceMod.WithHardCaps(0, 1);
        }

        // Critical Damage
        public float BaseCriticalDamage { get; set; } = 2;
        public float FlatCriticalDamageMod { get; set; }
        public float TotalCriticalDamage => BaseCriticalDamage + FlatCriticalDamageMod;

        // Attack Damage
        public float Level1AttackDamage { get; set; }
        public float AttackDamageGrowth { get; set; }
        public float FlatAttackDamageMod { get; set; }
        public float PercentAttackDamageMod { get; set; }

        public float BaseAttackDamage => Level1AttackDamage + AttackDamageGrowth * (Level - 1);
        public float TotalAttackDamage => (BaseAttackDamage + FlatAttackDamageMod) * (1 + PercentAttackDamageMod);
        public float BonusAttackDamage => TotalAttackDamage - BaseAttackDamage;

        // Armor
        public float Level1Armor { get; set; }
        public float ArmorGrowth { get; set; }
        public float FlatArmorMod { get; set; }

        public float BaseArmor => Level1Armor + ArmorGrowth * (Level - 1);
        public float BonusArmor => FlatArmorMod * PercentBonusArmorPenetration;
        public float TotalArmor => (BaseArmor + BonusArmor - FlatArmorReduction) * PercentArmorReduction;

        // Armor Penetration
        public float FlatArmorReduction { get; set; }
        public float PercentArmorReduction { get; private set; }
        public float PercentArmorPenetration { get; private set; }
        public float PercentBonusArmorPenetration { get; private set; }
        public float FlatArmorPenetration { get; set; }

        public void AddPercentArmorReduction(float percent)
        {
            PercentArmorReduction *= 1 - percent;
        }

        public void RemovePercentArmorReduction(float percent)
        {
            PercentArmorReduction /= 1 - percent;
        }

        public void AddPercentArmorPenetration(float percent)
        {
            PercentArmorPenetration *= 1 - percent;
        }

        public void RemovePercentArmorPenetration(float percent)
        {
            PercentArmorPenetration /= 1 - percent;
        }

        public void AddPercentBonusArmorPenetration(float percent)
        {
            PercentBonusArmorPenetration *= 1 - percent;
        }

        public void RemovePercentBonusArmorPenetration(float percent)
        {
            PercentBonusArmorPenetration /= 1 - percent;
        }

        // Magic Resist
        public float Level1MagicResist { get; set; }
        public float MagicResistGrowth { get; set; }
        public float FlatMagicResistMod { get; set; }

        public float BaseMagicResist => Level1MagicResist + (Level - 1) * MagicResistGrowth;
        public float BonusMagicResist => FlatMagicResistMod * PercentBonusMagicPenetration;
        public float TotalMagicResist => (BaseMagicResist + BonusMagicResist - FlatMagicReduction) * PercentMagicReduction;

        // Magic Penetration
        public float FlatMagicReduction { get; set; }
        public float PercentMagicReduction { get; private set; } = 1;
        public float PercentMagicPenetration { get; private set; } = 1;
        public float PercentBonusMagicPenetration { get; private set; } = 1;
        public float FlatMagicPenetration { get; set; }

        public void AddPercentMagicReduction(float percent)
        {
            PercentMagicReduction *= 1 - percent;
        }

        public void RemovePercentMagicReduction(float percent)
        {
            PercentArmorReduction /= 1 - percent;
        }

        public void AddPercentMagicPenetration(float percent)
        {
            PercentMagicPenetration *= 1 - percent;
        }

        public void RemovePercentMagicPenetration(float percent)
        {
            PercentMagicPenetration /= 1 - percent;
        }

        public void AddPercentBonusMagicPenetration(float percent)
        {
            PercentBonusMagicPenetration *= 1 - percent;
        }

        public void RemovePercentBonusMagicPenetration(float percent)
        {
            PercentBonusMagicPenetration /= 1 - percent;
        }

        // Life Steal
        public float LifeSteal { get; set; }

        // Spell Vamp
        public float SpellVamp { get; set; }
        
        // Ability Power
        public float FlatAbilityPower { get; set; }
        public float PercentAbilityPower { get; set; }
        public float TotalAbilityPower => FlatAbilityPower * (1 + PercentAbilityPower);

        // Dodge Chance
        private float _dodgeChance;
        public float DodgeChance
        {
            get => _dodgeChance.WithHardCaps(0, 1);
            set => _dodgeChance = value;
        }

        // Health Regeneration
        public float Level1HealthRegen { get; set; }
        public float HealthRegenGrowth { get; set; }
        public float FlatHealthRegenMod { get; set; }
        public float BaseHealthRegen => Level1HealthRegen + (Level - 1) * HealthRegenGrowth;
        public float TotalHealthRegen => BaseHealthRegen + FlatHealthRegenMod;

        // Mana Regeneration
        public float Level1ParRegen { get; set; }
        public float ParRegenGrowth { get; set; }
        public float FlatParRegenMod { get; set; }
        public float BaseParRegen => Level1ParRegen + (Level - 1) * ParRegenGrowth;
        public float TotalParRegen => BaseParRegen + FlatParRegenMod;

        // Cooldown Reduction
        public float CooldownReductionCap { get; set; } = 0.4f;
        public float FlatCooldownReduction { get; set; }
        public float CooldownReduction => Math.Max(CooldownReductionCap, FlatCooldownReduction);

        // Tenacity
        public float Tenacity { get; private set; } = 1;

        public void AddTenacity(float percent)
        {
            Tenacity *= 1 - percent;
        }

        public void RemoveTenacity(float percent)
        {
            Tenacity /= 1 - percent;
        }

        // Vision Range
        public float FlatSightRangeMod { get; set; }
        public float PercentSightRangeMod { get; set; }

        // Pathfinding Radius
        public float FlatPathfindingRadiusMod { get; set; }

        // Size
        public float BaseSize { get; set; } = 1;
        public float FlatSizeMod { get; set; }
        public float PercentSizeMod { get; set; }
        public float TotalSize => (BaseSize + FlatSizeMod) * (1 + PercentSizeMod);

        public bool GetActionState(ActionState state)
        {
            return ActionState.HasFlag(state);
        }

        public void SetActionState(ActionState state, bool value)
        {
            if (value)
            {
                ActionState |= state;
            }
            else
            {
                ActionState &= ~state;
            }
        }

        public float GetBaseManaCost(byte slot)
        {
            if (slot < 4)
            {
                return ManaCost[slot];
            }

            if (slot > 44 && slot < 61)
            {
                return ManaCostEx[slot - 45];
            }

            return 0;
        }

        public void SetBaseManaCost(byte slot, float val)
        {
            if (slot < 4)
            {
                ManaCost[slot] = val;
            }

            if (slot > 44 && slot < 61)
            {
                ManaCostEx[slot - 45] = val;
            }
        }

        public bool GetSpellEnabled(byte slot)
        {
            return (SpellEnabledBitFieldLower1 & (1 << slot)) != 0;
        }

        public void SetSpellEnabled(byte slot, bool enabled)
        {
            if (enabled)
            {
                SpellEnabledBitFieldLower1 |= (uint)(1 << slot);
            }
            else
            {
                SpellEnabledBitFieldLower1 &= (uint)~(1 << slot);
            }
        }

        public bool GetSummonerSpellEnabled(byte slot)
        {
            return (SpellEnabledBitFieldLower2 & (1 << slot)) != 0;
        }

        public void SetSummonerSpellEnabled(byte slot, bool enabled)
        {
            if (enabled)
            {
                SpellEnabledBitFieldLower2 |= (uint)(16 << slot);
            }
            else
            {
                SpellEnabledBitFieldLower2 &= (uint)~(16 << slot);
            }
        }

        public void LoadStats(string model, CharData charData, int skinId = 0)
        {
            Level1Health = charData.BaseHP;
            Level1Par = charData.BaseMP;
            Level1AttackDamage = charData.BaseDamage;
            BaseAttackRange = charData.AttackRange;
            BaseMovementSpeed = charData.MoveSpeed;
            Level1Armor = charData.Armor;
            Level1MagicResist = charData.SpellBlock;
            Level1HealthRegen = charData.BaseStaticHPRegen;
            Level1ParRegen = charData.BaseStaticMPRegen;
            AttackDelay = charData.AttackDelayOffsetPercent;
            HealthGrowth = charData.HPPerLevel;
            ParGrowth = charData.MPPerLevel;
            AttackDamageGrowth = charData.DamagePerLevel;
            ArmorGrowth = charData.ArmorPerLevel;
            MagicResistGrowth = charData.SpellBlockPerLevel;
            HealthRegenGrowth = charData.HPRegenPerLevel;
            ParRegenGrowth = charData.MPRegenPerLevel;
            AttackSpeedGrowth = charData.AttackSpeedPerLevel;

            var game = Program.ResolveDependency<Game>();
            var logger = Program.ResolveDependency<Logger>();
            var file = new ContentFile();
            try
            {
                var path = game.Config.ContentManager.GetUnitStatPath(model);
                logger.LogCoreInfo($"Loading {model}'s stats from path: {Path.GetFullPath(path)}!");
                var text = File.ReadAllText(Path.GetFullPath(path));
                file = JsonConvert.DeserializeObject<ContentFile>(text);
                if (file.Values.ContainsKey($"MeshSkin{(skinId == 0 ? "" : skinId.ToString())}"))
                {
                    BaseSize = file.GetFloat("MeshSkin", "SkinScale", 1);
                    return;
                }

                path = game.Config.ContentManager.GetUnitSkinPath(model, skinId);
                text = File.ReadAllText(Path.GetFullPath(path));
                file = JsonConvert.DeserializeObject<ContentFile>(text);
            }
            catch (ContentNotFoundException notfound)
            {
                logger.LogCoreWarning($"Stats for {model} was not found: {notfound.Message}");
                return;
            }

            BaseSize = file.GetFloat("MeshSkin", "SkinScale", 1);
        }

        public void ApplyItemValues(ItemType item)
        {
            // todo
            /*item.FlatArmorMod;
            item.PercentArmorMod
            item.FlatCritChanceMod
            item.FlatCritDamageMod
            item.PercentCritDamageMod
            item.FlatHPPoolMod
            item.PercentHPPoolMod
            item.FlatMPPoolMod
            item.PercentMPPoolMod
            item.FlatMagicDamageMod
            item.PercentMagicDamageMod
            item.FlatMagicPenetrationMod
            item.FlatMovementSpeedMod
            item.PercentMovementSpeedMod
            item.FlatPhysicalDamageMod
            item.PercentPhysicalDamageMod
            item.FlatSpellBlockMod
            item.PercentSpellBlockMod
            item.PercentAttackSpeedMod
            item.PercentBaseHPRegenMod
            item.PercentBaseMPRegenMod*/
        }

        public void RemoveItemValues(ItemType item)
        {
            // todo
            /*item.FlatArmorMod;
            item.PercentArmorMod
            item.FlatCritChanceMod
            item.FlatCritDamageMod
            item.PercentCritDamageMod
            item.FlatHPPoolMod
            item.PercentHPPoolMod
            item.FlatMPPoolMod
            item.PercentMPPoolMod
            item.FlatMagicDamageMod
            item.PercentMagicDamageMod
            item.FlatMagicPenetrationMod
            item.FlatMovementSpeedMod
            item.PercentMovementSpeedMod
            item.FlatPhysicalDamageMod
            item.PercentPhysicalDamageMod
            item.FlatSpellBlockMod
            item.PercentSpellBlockMod
            item.PercentAttackSpeedMod
            item.PercentBaseHPRegenMod
            item.PercentBaseMPRegenMod*/
        }
    }

    [Flags]
    public enum ActionState : uint
    {
        CanAttack = 1 << 0,
        CanCast = 1 << 1,
        CanMove = 1 << 2,
        CanNotMove = 1 << 3,
        Stealthed = 1 << 4,
        RevealSpecificUnit = 1 << 5,
        Taunted = 1 << 6,
        Feared = 1 << 7,
        IsFleeing = 1 << 8,
        CanNotAttack = 1 << 9,
        IsAsleep = 1 << 10,
        IsNearSighted = 1 << 11,
        IsGhosted = 1 << 12,

        Charmed = 1 << 15,
        NoRender = 1 << 16,
        ForceRenderParticles = 1 << 17,

        Unknown = 1 << 23 // set to 1 by default
    }

    [Flags]
    public enum IsTargetableToTeamFlags : uint
    {
        NonTargetableAlly = 0x800000,
        NonTargetableEnemy = 0x1000000,
        TargetableToAll = 0x2000000
    }

    public enum PrimaryAbilityResourceType : byte
    {
        Mana = 0,
        Energy = 1,
        None = 2,
        Shield = 3,
        BattleFury = 4,
        DragonFury = 5,
        Rage = 6,
        Heat = 7,
        Ferocity = 8,
        BloodWell = 9,
        Wind = 10,
        Other = 11
    }
}
