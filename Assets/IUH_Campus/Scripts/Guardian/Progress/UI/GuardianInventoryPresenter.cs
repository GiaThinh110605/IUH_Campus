using System;
using System.Collections.Generic;
using System.Linq;
namespace IUHCampus.Guardian.Progress.UI
{
 public interface IGuardianPlantingContext { bool CanPlant(string itemId); void Plant(string itemId); }
 public sealed class GuardianInventoryRow { public GuardianItemDefinition Definition; public int Quantity; }
 public static class GuardianInventoryPresenter
 {
  public static IReadOnlyList<GuardianInventoryRow> Rows(GuardianInventory inventory, GuardianItemDatabase database) => inventory.Items.Select(pair=>new GuardianInventoryRow{Definition=database.Find(pair.Key),Quantity=pair.Value}).Where(x=>x.Definition!=null).OrderBy(x=>x.Definition.category).ThenBy(x=>x.Definition.displayName,StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"),false)).ThenBy(x=>x.Definition.id,StringComparer.Ordinal).ToArray();
  public static string ActionLabel(GuardianItemDefinition item) => item.category==GuardianItemCategory.Seed?"Trồng":item.category==GuardianItemCategory.Consumable?"Dùng":"";
  public static bool CanAct(GuardianItemDefinition item, IGuardianPlantingContext context) => item.category==GuardianItemCategory.Seed && context!=null && context.CanPlant(item.id);
 }
}
