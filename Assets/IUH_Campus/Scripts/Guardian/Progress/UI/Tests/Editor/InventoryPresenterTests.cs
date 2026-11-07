using System;
using NUnit.Framework;
using UnityEngine;
namespace IUHCampus.Guardian.Progress.UI.Tests
{
 public class InventoryPresenterTests
 {
  [Test] public void MappingSortingAndActionState(){var db=ScriptableObject.CreateInstance<GuardianItemDatabase>();var a=ScriptableObject.CreateInstance<GuardianItemDefinition>();var b=ScriptableObject.CreateInstance<GuardianItemDefinition>();try{a.id="tool";a.displayName="Bình tưới";a.category=GuardianItemCategory.Tool;b.id="seed";b.displayName="Hạt hoa hồng";b.category=GuardianItemCategory.Seed;db.definitions.Add(a);db.definitions.Add(b);var inventory=new GuardianInventory(new GuardianProgressState(),db);inventory.Add(a.id,1);inventory.Add(b.id,2);var rows=GuardianInventoryPresenter.Rows(inventory,db);Assert.AreEqual("seed",rows[0].Definition.id);Assert.AreEqual(2,rows[0].Quantity);Assert.AreEqual("",GuardianInventoryPresenter.ActionLabel(a));Assert.AreEqual("Trồng",GuardianInventoryPresenter.ActionLabel(b));Assert.IsFalse(GuardianInventoryPresenter.CanAct(b,null));Assert.IsTrue(GuardianInventoryPresenter.CanAct(b,new Context()));a.category=GuardianItemCategory.Consumable;Assert.AreEqual("Dùng",GuardianInventoryPresenter.ActionLabel(a));Assert.IsFalse(GuardianInventoryPresenter.CanAct(a,new Context()));}finally{UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);UnityEngine.Object.DestroyImmediate(db);}}
  [Test] public void SameCategorySortsByVietnameseName(){var db=ScriptableObject.CreateInstance<GuardianItemDatabase>();var a=ScriptableObject.CreateInstance<GuardianItemDefinition>();var b=ScriptableObject.CreateInstance<GuardianItemDefinition>();try{a.id="rose";a.displayName="Hạt hoa hồng";b.id="daisy";b.displayName="Hạt hoa cúc";db.definitions.Add(a);db.definitions.Add(b);var inventory=new GuardianInventory(new GuardianProgressState(),db);inventory.Add(a.id,1);inventory.Add(b.id,1);Assert.AreEqual("daisy",GuardianInventoryPresenter.Rows(inventory,db)[0].Definition.id);}finally{UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);UnityEngine.Object.DestroyImmediate(db);}}
  sealed class Context:IGuardianPlantingContext{public bool CanPlant(string id)=>true;public void Plant(string id){}}
 }
}
