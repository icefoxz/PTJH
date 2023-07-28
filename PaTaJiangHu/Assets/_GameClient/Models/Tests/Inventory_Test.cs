using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.Core.Dizi;
using GameClient.Models;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using UnityEngine;

public class InventoryTest
{
    [Test]
    public void AddItem_ShouldAddToTemp_WhenLimitExceeded()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var item1 = new Equipment();
        var item2 = new Equipment();

        // Act
        inventory.AddItem(item1, out var item1IsPutToTemp);
        inventory.AddItem(item2, out var item2IsPutToTemp);

        // Assert
        Assert.AreEqual(1, inventory.GetAllEquipments().Count); // One item should be in Equipments list
        Assert.AreEqual(1, inventory.GetAllTempItems().Count); // Another one should be in TempItems list
        Assert.IsFalse(item1IsPutToTemp); // item1IsPutToTemp should be false
        Assert.IsTrue(item2IsPutToTemp); // item2IsPutToTemp should be true
    }

    [Test]
    public void AddItem_ShouldAddToEquipments_WhenLimitNotExceeded()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 2 } });
        var item = new Equipment();

        // Act
        inventory.AddItem(item, out var isPutToTemp);

        // Assert
        Assert.AreEqual(1, inventory.GetAllEquipments().Count);
        Assert.AreEqual(0, inventory.GetAllTempItems().Count);
        Assert.IsFalse(isPutToTemp);
    }

    [Test]
    public void AddItem_ShouldThrowException_WhenUnsupportedItemType()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var item = new UnsupportedGameItem();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => inventory.AddItem(item, out _));
    }

    [Test]
    public void AddItem_ShouldNotThrowException_WhenItemPackage()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var itemPackage = new ItemPackage();

        // Act
        inventory.AddItem(itemPackage);

        // Assert
        Assert.AreEqual(1, inventory.GetAllPackages().Count);
    }

    [Test]
    public void AddItem_ShouldAddToTemp_WhenLimitReached()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var item1 = new Equipment();
        var item2 = new Equipment();

        // Act
        inventory.AddItem(item1, out var item1IsPutToTemp);
        inventory.AddItem(item2, out var item2IsPutToTemp);

        // Assert
        Assert.AreEqual(1, inventory.GetAllTempItems().Count); // The second item should be added to TempItems list
        Assert.IsTrue(item1IsPutToTemp); // item1IsPutToTemp should be false
        Assert.IsTrue(item2IsPutToTemp); // item2IsPutToTemp should be true
    }

    [Test]
    public void RemoveItem_ShouldAddTempItemToEquipments_WhenTempItemsExist()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var item1 = new Equipment();
        var item2 = new Equipment();
        inventory.AddItem(item1, out _);
        inventory.AddItem(item2, out _);

        // Act
        inventory.RemoveItem(item1,out var isTempItemReplace);

        // Assert
        Assert.AreEqual(1, inventory.GetAllEquipments().Count); // The second item should be moved from TempItems to Equipments list
        Assert.AreEqual(0, inventory.GetAllTempItems().Count); // TempItems list should be empty

        Assert.IsTrue(isTempItemReplace); // isTempItemReplace should be true
        Assert.IsTrue(inventory.GetAllEquipments().First() == item2); // The second item should be in Equipments list
    }

    [Test]
    public void RemoveItem_ShouldNotAddTempItemToEquipments_WhenTempItemsNotExist()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var item = new Equipment();
        inventory.AddItem(item, out _);
        
        // Act
        inventory.RemoveItem(item,out var isTempItemReplace);

        // Assert
        Assert.AreEqual(0, inventory.GetAllEquipments().Count); // Equipments list should be empty
        Assert.AreEqual(0, inventory.GetAllTempItems().Count); // TempItems list should be empty
        Assert.IsFalse(isTempItemReplace); // isTempItemReplace should be false
    }

    [Test]
    public void RemoveItem_ShouldThrowException_WhenUnsupportedItemType()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var item = new UnsupportedGameItem();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.RemoveItem(item, out _));
    }

    [Test]
    public void RemoveItem_ShouldNotThrowException_WhenItemPackage()
    {
        // Arrange
        var inventory = new Inventory(new Dictionary<ItemType, int> { { ItemType.Equipment, 1 } });
        var itemPackage = new ItemPackage();
        inventory.AddItem(itemPackage);

        // Act
        inventory.RemoveItem(itemPackage);

        // Assert
        Assert.AreEqual(0, inventory.GetAllPackages().Count);
    }
}

public class ItemPackage : IItemPackage
{
    public int Grade { get; }
    public IGameItem[] AllItems { get; }
    public IResPackage Package { get; }
}

public class UnsupportedGameItem : IGameItem
{
    public UnsupportedGameItem()
    {
        Type = (ItemType) 99;
    }

    public int Id { get; }
    public Sprite Icon { get; }
    public string Name { get; }
    public string About { get; }
    public ItemType Type { get; }
}

public class Equipment : IEquipment
{
    public int Id { get; }
    public Sprite Icon { get; }
    public string Name { get; }
    public string About { get; }
    public ItemType Type { get; }
    public EquipKinds EquipKind { get; }
    public ColorGrade Grade { get; }
    public float GetAddOn(DiziProps prop)
    {
        throw new System.NotImplementedException();
    }

    public int Quality { get; }
    public ICombatSet GetCombatSet()
    {
        throw new System.NotImplementedException();
    }
}