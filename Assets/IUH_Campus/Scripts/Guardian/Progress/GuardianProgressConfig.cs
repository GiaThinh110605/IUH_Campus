using System;
using System.Collections.Generic;
using UnityEngine;
namespace IUHCampus.Guardian.Progress {
 [Serializable] public sealed class GuardianUpgradeLevel {public int coreCost;public float increase;}
 [Serializable] public sealed class GuardianItemPrice {public string itemId;public int corePrice;}
 [CreateAssetMenu(menuName="IUH Guardian/Progress Config")]
 public sealed class GuardianProgressConfig : ScriptableObject {
  [Min(0.01f)] public float stageMinutes=1,fertilizerMultiplier=2;
  [Min(1)] public int plotCount=6;
  public int coreRewardOnBloom=5;
  [Min(0)] public float saveDebounceSeconds=0.5f;
  public List<GuardianItemStack> startingItems=new List<GuardianItemStack>{new GuardianItemStack{itemId="tool_watering_can",qty=1}};
  public string[] randomSeedIds={"seed_sunflower","seed_rose","seed_daisy"};public int startingRandomSeeds=2;
  public List<GuardianItemPrice> prices=new List<GuardianItemPrice>();
  public GuardianUpgradeLevel[] attack=new GuardianUpgradeLevel[2],defense=new GuardianUpgradeLevel[2],vitality=new GuardianUpgradeLevel[2];
 }
}
