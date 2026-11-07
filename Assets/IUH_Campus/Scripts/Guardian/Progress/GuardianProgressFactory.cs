using System;
namespace IUHCampus.Guardian.Progress {
 public static class GuardianProgressFactory {
  public const string StarterReward="guardian:starter:v1";
  public static GuardianProgressState Create(GuardianProgressConfig config,GuardianItemDatabase database,Random random=null){
   if(config.plotCount<1||config.startingRandomSeeds<0||config.stageMinutes<=0||config.fertilizerMultiplier<=0)throw new ArgumentException("Invalid progress config.");
   var state=new GuardianProgressState();var inventory=new GuardianInventory(state,database);random=random??new Random();
   foreach(var s in config.startingItems)if(!inventory.Add(s.itemId,s.qty))throw new ArgumentException("Invalid starter items.");
   if(config.startingRandomSeeds>0&&(config.randomSeedIds==null||config.randomSeedIds.Length==0))throw new ArgumentException("Missing starter seeds.");
   for(int i=0;i<config.startingRandomSeeds;i++)if(!inventory.Add(config.randomSeedIds[random.Next(config.randomSeedIds.Length)],1))throw new ArgumentException("Invalid starter seed capacity.");
   for(int i=0;i<config.plotCount;i++)state.gardenPlots.Add(new GuardianGardenPlot{plotId="plot_"+i,stage=GuardianPlantStage.Empty});
   state.appliedRewardIds.Add(StarterReward);return state;
  }
 }
}
