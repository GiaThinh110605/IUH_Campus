using System;
using System.Collections.Generic;
namespace IUHCampus.Guardian.Progress {
 public enum GuardianPlantStage { Empty, Seed, Sprout, Bloom }
 [Serializable] public sealed class GuardianItemStack {public string itemId;public int qty;}
 [Serializable] public sealed class GuardianUpgrades {public int attack,defense,vitality;}
 [Serializable] public sealed class GuardianGardenPlot {public string plotId,flowerType,plantedAtUtc,stageStartedAtUtc;public GuardianPlantStage stage;public bool wateredThisStage,fertilizedThisStage;}
 [Serializable] public sealed class GuardianProgressState {
  public const int CurrentVersion=1;
  public int version=CurrentVersion,campusCore;
  public List<GuardianItemStack> inventory=new List<GuardianItemStack>();
  public GuardianUpgrades upgrades=new GuardianUpgrades();
  public List<GuardianGardenPlot> gardenPlots=new List<GuardianGardenPlot>();
  public List<string> appliedRewardIds=new List<string>(),purifiedAreas=new List<string>();
  public void Validate(){if(version!=CurrentVersion||campusCore<0||inventory==null||upgrades==null||gardenPlots==null||appliedRewardIds==null||purifiedAreas==null)throw new ArgumentException("Invalid progress version/state.");
   if(upgrades.attack<0||upgrades.attack>2||upgrades.defense<0||upgrades.defense>2||upgrades.vitality<0||upgrades.vitality>2)throw new ArgumentException("Invalid upgrades.");
   var ids=new HashSet<string>();foreach(var s in inventory)if(s==null||string.IsNullOrWhiteSpace(s.itemId)||s.qty<=0||!ids.Add(s.itemId))throw new ArgumentException("Invalid inventory.");
  }
 }
}
