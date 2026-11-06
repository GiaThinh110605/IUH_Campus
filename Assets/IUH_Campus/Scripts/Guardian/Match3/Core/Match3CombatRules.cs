using System;

namespace IUHCampus.Guardian.Match3
{
    public enum CombatOutcome
    {
        Ongoing = 0,
        Victory = 1,
        Defeat = 2
    }

    /// <summary>Balance numbers for one fight. Defaults target the 1000 vs 800 HP scale.</summary>
    [Serializable]
    public sealed class Match3CombatConfig
    {
        public int PlayerMaxHp = 1000;
        public int PlayerMaxEnergy = 100;
        public int PlayerMaxShield = 300;

        public int BossMaxHp = 800;
        public int BossMinDmg = 65;
        public int BossMaxDmg = 90;

        /// <summary>Every Nth boss turn is a telegraphed charged attack.</summary>
        public int ChargeEveryNTurns = 3;
        public float ChargeMultiplier = 1.6f;

        public int DmgPerSword = 22;
        public int ShieldPerTile = 15;
        public int EnergyPerTile = 10;
        public int HealPerTile = 14;
        public int PurifyIntentPerTile = 10;
        public int PurifyDmgPerTile = 6;

        /// <summary>Each cascade after the first adds this much to the multiplier.</summary>
        public float ComboStepBonus = 0.25f;
        public float MaxComboMultiplier = 2f;
        /// <summary>Extra multiplier when a step contains a line of 4 or more.</summary>
        public float BigMatchBonus = 0.5f;

        public int SkillDamage = 160;
    }

    public struct StepResult
    {
        public int Damage;
        public int ShieldGained;
        public int EnergyGained;
        public int Healed;
        public int IntentReduced;
        public float Multiplier;
        public int ComboIndex;
    }

    public struct BossTurnResult
    {
        public int Intent;
        public int Blocked;
        public int Damage;
        public bool WasCharged;
    }

    /// <summary>
    /// Pure combat state and rules, no Unity dependency, so it can be unit tested.
    /// Player damage lands per cascade step; the boss acts once after each completed swap.
    /// </summary>
    public sealed class Match3CombatRules
    {
        private readonly Match3CombatConfig m_Config;
        private readonly Random m_Random;

        public int PlayerHp { get; private set; }
        public int PlayerShield { get; private set; }
        public int PlayerEnergy { get; private set; }
        public int BossHp { get; private set; }
        public int BossIntent { get; private set; }
        public bool IsBossCharging { get; private set; }
        public int BossTurnsTaken { get; private set; }
        public CombatOutcome Outcome { get; private set; }

        public Match3CombatConfig Config => m_Config;
        public bool IsOver => Outcome != CombatOutcome.Ongoing;
        public bool CanCastSkill => !IsOver && PlayerEnergy >= m_Config.PlayerMaxEnergy;

        public Match3CombatRules(Match3CombatConfig config, int? seed = null)
        {
            m_Config = config ?? throw new ArgumentNullException(nameof(config));
            m_Random = seed.HasValue ? new Random(seed.Value) : new Random();
            Reset();
        }

        public void Reset()
        {
            PlayerHp = m_Config.PlayerMaxHp;
            PlayerShield = 0;
            PlayerEnergy = 0;
            BossHp = m_Config.BossMaxHp;
            BossTurnsTaken = 0;
            Outcome = CombatOutcome.Ongoing;
            RollIntent();
        }

        public float MultiplierFor(CascadeStep step)
        {
            float combo = 1f + m_Config.ComboStepBonus * Math.Max(0, step.ComboIndex);
            if (combo > m_Config.MaxComboMultiplier) combo = m_Config.MaxComboMultiplier;
            float big = step.LongestMatch >= 4 ? 1f + m_Config.BigMatchBonus : 1f;
            return combo * big;
        }

        public StepResult ApplyStep(CascadeStep step)
        {
            var result = new StepResult { ComboIndex = step.ComboIndex };
            if (IsOver || step == null) return result;

            float mult = MultiplierFor(step);
            result.Multiplier = mult;

            result.Damage = Scale(step.SwordCount * m_Config.DmgPerSword + step.PurifyCount * m_Config.PurifyDmgPerTile, mult);

            int shield = Scale(step.ShieldCount * m_Config.ShieldPerTile, mult);
            int newShield = Math.Min(m_Config.PlayerMaxShield, PlayerShield + shield);
            result.ShieldGained = newShield - PlayerShield;
            PlayerShield = newShield;

            int newEnergy = Math.Min(m_Config.PlayerMaxEnergy, PlayerEnergy + step.EnergyCount * m_Config.EnergyPerTile);
            result.EnergyGained = newEnergy - PlayerEnergy;
            PlayerEnergy = newEnergy;

            int newHp = Math.Min(m_Config.PlayerMaxHp, PlayerHp + Scale(step.HealCount * m_Config.HealPerTile, mult));
            result.Healed = newHp - PlayerHp;
            PlayerHp = newHp;

            int reduce = Math.Min(BossIntent, Scale(step.PurifyCount * m_Config.PurifyIntentPerTile, mult));
            result.IntentReduced = reduce;
            BossIntent -= reduce;

            DamageBoss(result.Damage);
            return result;
        }

        public int CastSkill()
        {
            if (!CanCastSkill) return 0;
            PlayerEnergy = 0;
            int dmg = Math.Min(BossHp, m_Config.SkillDamage);
            DamageBoss(m_Config.SkillDamage);
            return dmg;
        }

        public BossTurnResult ResolveBossTurn()
        {
            var result = new BossTurnResult { Intent = BossIntent, WasCharged = IsBossCharging };
            if (IsOver) return result;

            result.Blocked = Math.Min(PlayerShield, BossIntent);
            result.Damage = BossIntent - result.Blocked;

            // Shield only protects against the attack it was built for.
            PlayerShield = 0;
            PlayerHp = Math.Max(0, PlayerHp - result.Damage);
            BossTurnsTaken++;

            if (PlayerHp <= 0)
            {
                Outcome = CombatOutcome.Defeat;
            }
            else
            {
                RollIntent();
            }
            return result;
        }

        public void TimeOut()
        {
            if (!IsOver) Outcome = CombatOutcome.Defeat;
        }

        private void DamageBoss(int amount)
        {
            if (amount <= 0) return;
            BossHp = Math.Max(0, BossHp - amount);
            if (BossHp <= 0) Outcome = CombatOutcome.Victory;
        }

        private void RollIntent()
        {
            int n = Math.Max(1, m_Config.ChargeEveryNTurns);
            IsBossCharging = n > 1 && BossTurnsTaken % n == n - 1;
            int baseDmg = m_Random.Next(m_Config.BossMinDmg, m_Config.BossMaxDmg + 1);
            BossIntent = IsBossCharging ? (int)Math.Round(baseDmg * m_Config.ChargeMultiplier) : baseDmg;
        }

        private static int Scale(int value, float mult) => (int)Math.Round(value * mult, MidpointRounding.AwayFromZero);
    }
}
