using System;
namespace IUHCampus.Guardian.Progress {
 public sealed class GuardianWallet {
  readonly GuardianProgressState state;public event Action Changed;public int Core=>state.campusCore;
  public GuardianWallet(GuardianProgressState state){this.state=state;}
  public bool AddCore(int amount,string rewardId){if(amount<=0||string.IsNullOrWhiteSpace(rewardId)||state.appliedRewardIds.Contains(rewardId)||amount>int.MaxValue-Core)return false;state.campusCore+=amount;state.appliedRewardIds.Add(rewardId);Changed?.Invoke();return true;}
  public bool SpendCore(int amount,string reason){if(amount<=0||string.IsNullOrWhiteSpace(reason)||amount>Core)return false;state.campusCore-=amount;Changed?.Invoke();return true;}
 }
}
