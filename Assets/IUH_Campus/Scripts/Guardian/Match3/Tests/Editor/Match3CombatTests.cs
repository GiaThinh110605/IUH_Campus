using System.Collections.Generic;
using NUnit.Framework;

namespace IUHCampus.Guardian.Match3.Tests
{
    public class Match3BoardModelTests
    {
        [Test]
        public void Board_Is6x5_AndStartsWithoutMatches()
        {
            Assert.AreEqual(6, Match3BoardModel.Width);
            Assert.AreEqual(5, Match3BoardModel.Height);

            for (int seed = 0; seed < 50; seed++)
            {
                var model = new Match3BoardModel(seed);
                model.Initialize();
                Assert.IsEmpty(model.FindMatches(), "seed " + seed);
                Assert.IsTrue(model.HasValidMoves(), "seed " + seed);
            }
        }

        [Test]
        public void Board_UsesAllFiveTileTypes()
        {
            CollectionAssert.AreEquivalent(
                new[] { TileType.Sword, TileType.Shield, TileType.Energy, TileType.Heal, TileType.Purify },
                Match3BoardModel.Playable);
        }

        [Test]
        public void ProcessStep_CountsPurifyTiles()
        {
            var model = new Match3BoardModel(1);
            model.Initialize();
            FillNoMatch(model);
            model.SetTile(0, 0, TileType.Purify);
            model.SetTile(1, 0, TileType.Purify);
            model.SetTile(2, 0, TileType.Purify);

            CascadeStep step = model.ProcessStep();

            Assert.IsNotNull(step);
            Assert.AreEqual(3, step.PurifyCount);
            Assert.AreEqual(3, step.LongestMatch);
        }

        [Test]
        public void ProcessStep_ReportsLineOfFour()
        {
            var model = new Match3BoardModel(2);
            model.Initialize();
            FillNoMatch(model);
            for (int x = 0; x < 4; x++) model.SetTile(x, 0, TileType.Sword);

            CascadeStep step = model.ProcessStep();

            Assert.AreEqual(4, step.SwordCount);
            Assert.AreEqual(4, step.LongestMatch);
        }

        [Test]
        public void ProcessStep_CountsPurifyInOverlappingBlastAreasOnce()
        {
            var model = new Match3BoardModel(3);
            FillNoMatch(model);
            for (int x = 0; x < 5; x++) model.SetTile(x, 0, TileType.Sword);
            model.SetTile(2, 1, TileType.Purify);

            CascadeStep step = model.ProcessStep();

            Assert.IsNotNull(step);
            Assert.IsTrue(step.HasMegaBomb);
            Assert.AreEqual(5, step.SwordCount);
            Assert.AreEqual(1, step.PurifyCount);
            Assert.AreEqual(1, step.AllBlastedCoords.FindAll(c => c.X == 2 && c.Y == 1).Count);
        }

        // Checkerboard of two non-adjacent patterns guarantees no line of three anywhere.
        private static void FillNoMatch(Match3BoardModel model)
        {
            TileType[] a = { TileType.Shield, TileType.Heal };
            TileType[] b = { TileType.Heal, TileType.Energy };
            for (int x = 0; x < Match3BoardModel.Width; x++)
            {
                for (int y = 0; y < Match3BoardModel.Height; y++)
                {
                    TileType[] row = (y % 2 == 0) ? a : b;
                    model.SetTile(x, y, row[x % 2]);
                }
            }
        }
    }

    public class Match3CombatRulesTests
    {
        private static Match3CombatConfig Config() => new Match3CombatConfig
        {
            BossMinDmg = 100,
            BossMaxDmg = 100,
            ChargeEveryNTurns = 3,
            ChargeMultiplier = 1.5f
        };

        private static CascadeStep Step(int sword = 0, int shield = 0, int energy = 0, int heal = 0, int purify = 0, int combo = 0, int line = 3)
        {
            var coords = new List<BoardCoord>();
            for (int i = 0; i < line; i++) coords.Add(new BoardCoord(i, 0));
            var matches = new List<MatchGroup> { new MatchGroup(TileType.Sword, coords) };
            return new CascadeStep(matches, new List<TileDrop>(), new List<TileSpawn>())
            {
                SwordCount = sword,
                ShieldCount = shield,
                EnergyCount = energy,
                HealCount = heal,
                PurifyCount = purify,
                ComboIndex = combo
            };
        }

        [Test]
        public void StartsAt1000vs800()
        {
            var rules = new Match3CombatRules(new Match3CombatConfig(), 1);
            Assert.AreEqual(1000, rules.PlayerHp);
            Assert.AreEqual(800, rules.BossHp);
            Assert.AreEqual(CombatOutcome.Ongoing, rules.Outcome);
        }

        [Test]
        public void SwordDamage_ScalesWithCombo_AndBigMatch()
        {
            var rules = new Match3CombatRules(Config(), 1);

            Assert.AreEqual(66, rules.ApplyStep(Step(sword: 3)).Damage);
            Assert.AreEqual(83, rules.ApplyStep(Step(sword: 3, combo: 1)).Damage);   // x1.25
            Assert.AreEqual(132, rules.ApplyStep(Step(sword: 4, line: 4)).Damage);   // x1.5
            Assert.AreEqual(800 - 66 - 83 - 132, rules.BossHp);
        }

        [Test]
        public void ComboMultiplier_IsCapped()
        {
            var rules = new Match3CombatRules(Config(), 1);
            Assert.AreEqual(2f, rules.MultiplierFor(Step(combo: 10)), 1e-4);
        }

        [Test]
        public void Shield_AbsorbsBossHit_ThenResets()
        {
            var rules = new Match3CombatRules(Config(), 1);
            rules.ApplyStep(Step(shield: 4)); // 60 shield

            BossTurnResult turn = rules.ResolveBossTurn();

            Assert.AreEqual(60, turn.Blocked);
            Assert.AreEqual(40, turn.Damage);
            Assert.AreEqual(960, rules.PlayerHp);
            Assert.AreEqual(0, rules.PlayerShield);
        }

        [Test]
        public void Purify_ReducesIntent_AndDealsSmallDamage()
        {
            var rules = new Match3CombatRules(Config(), 1);
            StepResult r = rules.ApplyStep(Step(purify: 3));

            Assert.AreEqual(30, r.IntentReduced);
            Assert.AreEqual(70, rules.BossIntent);
            Assert.AreEqual(18, r.Damage);
        }

        [Test]
        public void ThirdBossTurn_IsCharged()
        {
            var rules = new Match3CombatRules(Config(), 1);
            Assert.IsFalse(rules.ResolveBossTurn().WasCharged);
            Assert.IsFalse(rules.ResolveBossTurn().WasCharged);
            Assert.IsTrue(rules.IsBossCharging);
            Assert.AreEqual(150, rules.BossIntent);
            Assert.IsTrue(rules.ResolveBossTurn().WasCharged);
        }

        [Test]
        public void Heal_AndEnergy_AreClamped()
        {
            var rules = new Match3CombatRules(Config(), 1);
            rules.ApplyStep(Step(heal: 5));
            Assert.AreEqual(1000, rules.PlayerHp);

            rules.ApplyStep(Step(energy: 20));
            Assert.AreEqual(100, rules.PlayerEnergy);
            Assert.IsTrue(rules.CanCastSkill);
        }

        [Test]
        public void Skill_SpendsEnergy_AndHitsBoss()
        {
            var rules = new Match3CombatRules(Config(), 1);
            Assert.AreEqual(0, rules.CastSkill());

            rules.ApplyStep(Step(energy: 10));
            Assert.AreEqual(160, rules.CastSkill());
            Assert.AreEqual(640, rules.BossHp);
            Assert.AreEqual(0, rules.PlayerEnergy);
        }

        [Test]
        public void BossAtZero_IsVictory_AndStopsBossTurn()
        {
            var rules = new Match3CombatRules(Config(), 1);
            rules.ApplyStep(Step(sword: 40));

            Assert.AreEqual(CombatOutcome.Victory, rules.Outcome);
            Assert.AreEqual(0, rules.ResolveBossTurn().Damage);
            Assert.AreEqual(1000, rules.PlayerHp);
        }

        [Test]
        public void PlayerAtZero_IsDefeat()
        {
            var config = Config();
            config.PlayerMaxHp = 150;
            var rules = new Match3CombatRules(config, 1);

            rules.ResolveBossTurn();
            rules.ResolveBossTurn();

            Assert.AreEqual(CombatOutcome.Defeat, rules.Outcome);
            Assert.AreEqual(0, rules.PlayerHp);
        }

        [Test]
        public void TimeOut_IsDefeat()
        {
            var rules = new Match3CombatRules(Config(), 1);
            rules.TimeOut();
            Assert.AreEqual(CombatOutcome.Defeat, rules.Outcome);
        }
    }
}
