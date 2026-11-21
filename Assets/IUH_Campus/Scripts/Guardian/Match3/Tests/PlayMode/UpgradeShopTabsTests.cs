using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class UpgradeShopTabsTests
{
    [UnityTest]
    public IEnumerator ShopOpensAndSwitchesBetweenUpgradesAndInventory()
    {
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        var shopType = Type.GetType("IUHCampus.Guardian.IUHUpgradeShopUI, Assembly-CSharp", true);
        var buttonType = Type.GetType("UnityEngine.UI.Button, UnityEngine.UI", true);
        var root = new GameObject("Upgrade shop tab fixture");
        root.SetActive(false);
        CursorLockMode lockMode = Cursor.lockState;
        bool cursorVisible = Cursor.visible;
        try
        {
            var shop = root.AddComponent(shopType);
            var panel = NewChild(root.transform, "Shop panel");
            var upgrades = NewChild(panel.transform, "Upgrades");
            var inventory = NewChild(panel.transform, "Inventory");
            var upgradeButton = NewChild(panel.transform, "Upgrade tab").AddComponent(buttonType);
            var inventoryButton = NewChild(panel.transform, "Inventory tab").AddComponent(buttonType);
            shopType.GetField("m_ShopPanel", fields).SetValue(shop, panel);
            shopType.GetField("m_TabUpgradesContent", fields).SetValue(shop, upgrades);
            shopType.GetField("m_TabInventoryContent", fields).SetValue(shop, inventory);
            shopType.GetField("m_BtnTabUpgrades", fields).SetValue(shop, upgradeButton);
            shopType.GetField("m_BtnTabInventory", fields).SetValue(shop, inventoryButton);
            root.SetActive(true);
            yield return null;

            var open = shopType.GetMethod("OpenShop", new[] { typeof(int) });
            var onClick = buttonType.GetProperty("onClick");
            open.Invoke(shop, new object[] { 0 });
            Assert.That(panel.activeInHierarchy && upgrades.activeInHierarchy, Is.True);
            Assert.That(inventory.activeInHierarchy, Is.False);

            ((UnityEvent)onClick.GetValue(inventoryButton)).Invoke();
            Assert.That(inventory.activeInHierarchy, Is.True);
            Assert.That(upgrades.activeInHierarchy, Is.False);

            ((UnityEvent)onClick.GetValue(upgradeButton)).Invoke();
            Assert.That(upgrades.activeInHierarchy, Is.True);
            Assert.That(inventory.activeInHierarchy, Is.False);

            open.Invoke(shop, new object[] { 2 });
            Assert.That(upgrades.activeInHierarchy, Is.True, "An unsupported tab index should open Upgrades.");
        }
        finally
        {
            Object.DestroyImmediate(root);
            Cursor.lockState = lockMode;
            Cursor.visible = cursorVisible;
        }
    }

    private static GameObject NewChild(Transform parent, string name)
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        return child;
    }
}
