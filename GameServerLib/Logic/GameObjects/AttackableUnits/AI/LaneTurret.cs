using LeagueSandbox.GameServer.Logic.Enet;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum TurretType
    {
        OuterTurret,
        InnerTurret,
        InhibitorTurret,
        NexusTurret,
        FountainTurret
    }

    public class LaneTurret : BaseTurret
    {
        public TurretType Type { get; private set; }
        private bool _turretHPUpdated;

        public LaneTurret(
            string name,
            float x = 0,
            float y = 0,
            TeamId team = TeamId.TEAM_BLUE,
            TurretType type = TurretType.OuterTurret,
            int[] items = null,
            uint netId = 0
        ) : base(name, "", x, y, team, netId)
        {
            Type = type;
            if (items != null)
            {
                foreach (var item in items)
                {
                    var itemTemplate = _itemManager.SafeGetItemType(item);
                    if (itemTemplate == null)
                    {
                        continue;
                    }
                    Inventory.AddItem(itemTemplate);
                }
            }

            BuildTurret(type);
        }

        public int GetEnemyChampionsCount()
        {
            var blueTeam = new List<Champion>();
            var purpTeam = new List<Champion>();
            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(TeamId.TEAM_BLUE))
            {
                blueTeam.Add(player);
            }

            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(TeamId.TEAM_PURPLE))
            {
                purpTeam.Add(player);
            }

            if (Team == TeamId.TEAM_BLUE)
            {
                return purpTeam.Count;
            }

            return blueTeam.Count;
        }

        public void BuildTurret(TurretType type)
        {
            switch (type)
            {
                case TurretType.InnerTurret:
                    globalGold = 100;

                    Stats.Level1Health = 1300;
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = 0;
                    Stats.Level1Armor = 60;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 170;
                    Stats.IsTargetableToTeam = IsTargetableToTeamFlags.NonTargetableEnemy;
                    Stats.IsInvulnerable = true;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.OuterTurret:
                    globalGold = 125;

                    Stats.Level1Health = 1300;
                    Stats.Level1AttackDamage = 100;
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;
                    Stats.Level1Armor = 60;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 152;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.InhibitorTurret:
                    globalGold = 150;
                    globalExp = 500;

                    Stats.Level1Health = 1300;
                    Stats.Level1HealthRegen = 5;
                    Stats.AddPercentArmorPenetration(0.825f);
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;
                    Stats.Level1Armor = 67;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 190;
                    Stats.IsTargetableToTeam = IsTargetableToTeamFlags.NonTargetableEnemy;
                    Stats.IsInvulnerable = true;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.NexusTurret:
                    globalGold = 50;

                    Stats.Level1Health = 1300;
                    Stats.Level1HealthRegen = 5;
                    Stats.AddPercentArmorPenetration(0.825f);
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;
                    Stats.Level1Armor = 65;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 180;
                    Stats.IsTargetableToTeam = IsTargetableToTeamFlags.NonTargetableEnemy;
                    Stats.IsInvulnerable = true;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.FountainTurret:
                    Stats.AttackDelay = -0.610f;
                    Stats.Level1Health = 9999;
                    Stats.Level1AttackDamage = 999;
                    Stats.BaseAttackRange = 1250;
                    SetTargetableToTeam(TeamId.TEAM_BLUE, false);
                    SetTargetableToTeam(TeamId.TEAM_PURPLE, false);
                    Stats.IsInvulnerable = true;

                    AutoAttackDelay = 1.0f / 30.0f;
                    AutoAttackProjectileSpeed = 2000.0f;
                    break;
                default:
                    Stats.Level1Health = 2000;
                    Stats.Level1AttackDamage = 100;
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;

                    break;
            }
        }

        public override void update(float diff)
        {
            //Update Stats if it's time
            switch (Type)
            {
                case TurretType.OuterTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.FlatHealthBonus += GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime > 40000 - GetEnemyChampionsCount() * 2000 &&
                        _game.GameTime < 400000 - GetEnemyChampionsCount() * 2000)
                    {
                        Stats.FlatMagicResistMod = (_game.GameTime - 30000) / 60000;
                        Stats.FlatAttackDamageMod = (_game.GameTime - 30000) / 60000 * 4;
                    }
                    else if (_game.GameTime >= 30000)
                    {
                        Stats.FlatMagicResistMod = 7;
                        Stats.FlatAttackDamageMod = 28;
                    }
                    break;
                case TurretType.InnerTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.FlatHealthBonus = GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime > 480000 && _game.GameTime < 1620000)
                    {
                        Stats.FlatArmorMod = (_game.GameTime - 480000) / 60000;
                        Stats.FlatMagicResistMod = (_game.GameTime - 480000) / 60000;
                        Stats.FlatAttackDamageMod = (_game.GameTime - 480000) / 60000 * 4;
                    }
                    else if (_game.GameTime >= 480000)
                    {
                        Stats.FlatArmorMod = 80;
                        Stats.FlatMagicResistMod = 120;
                        Stats.FlatAttackDamageMod = 250;
                    }
                    break;
                case TurretType.InhibitorTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.FlatHealthBonus = GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime > 480000 && _game.GameTime < 2220000)
                    {
                        Stats.FlatArmorMod = (_game.GameTime - 480000) / 60000;
                        Stats.FlatMagicResistMod = (_game.GameTime - 480000) / 60000;
                        Stats.FlatAttackDamageMod = (_game.GameTime - 480000) / 60000 * 4;
                    }
                    else if (_game.GameTime >= 480000)
                    {
                        Stats.FlatArmorMod = 97;
                        Stats.FlatMagicResistMod = 130;
                        Stats.FlatAttackDamageMod = 250;
                    }
                    break;
                case TurretType.NexusTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.FlatHealthBonus = GetEnemyChampionsCount() * 125;
                    }

                    if (_game.GameTime > 480000 && _game.GameTime < 2220000)
                    {
                        Stats.FlatArmorMod = (_game.GameTime - 480000) / 60000;
                        Stats.FlatMagicResistMod = (_game.GameTime - 480000) / 60000;
                        Stats.FlatAttackDamageMod = (_game.GameTime - 480000) / 60000 * 4;
                    }
                    else if (_game.GameTime >= 480000)
                    {
                        Stats.FlatArmorMod = 95;
                        Stats.FlatMagicResistMod = 130;
                        Stats.FlatAttackDamageMod = 300;
                    }

                    break;
            }
            _turretHPUpdated = true;
            base.update(diff);
        }

        public override void refreshWaypoints()
        {
        }

        public override void AutoAttackHit(AttackableUnit target)
        {
            if (Type == TurretType.FountainTurret)
            {
                target.TakeDamage(this, 1000, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);
            }
            else
            {
                base.AutoAttackHit(target);
            }
        }
    }
}
