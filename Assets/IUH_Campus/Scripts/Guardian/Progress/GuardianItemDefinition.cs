using UnityEngine;
namespace IUHCampus.Guardian.Progress {
 public enum GuardianItemCategory { Seed, Tool, Consumable }
 [CreateAssetMenu(menuName="IUH Guardian/Item")]
 public sealed class GuardianItemDefinition : ScriptableObject {
  public string id, displayName, description, flowerType;
  public Sprite icon;
  public GuardianItemCategory category;
  public bool stackable=true;
  [Min(1)] public int maxStack=99;
 }
}
