using Godot;
using Godot.Collections;
using QFramework;
using System;

namespace GridBaseInventorySystem;

/// <summary>
/// 背包业务类
/// </summary>
public partial class InventoryService : BaseContainerService
{

	/// <summary>
	/// 向背包添加物品
	/// 如果是可堆叠物品，如果当前数量大于可堆叠数量，会重置为允许的最大值，成功后发射信号 sig_inv_item_updated
	/// 如果是不可堆叠物品，或堆叠后还有剩余，成功后发射 sig_inv_item_added
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public bool AddItem(string invName, ItemData itemData)
	{
		var newItemData = (ItemData)itemData.Duplicate();
		if (newItemData is StackableData stackableNew)
		{
			if (stackableNew.CurrentAmount > stackableNew.StackSize)
			{
				stackableNew.CurrentAmount = stackableNew.StackSize;
			}
			var items = FindItemDataByItemName(invName, newItemData.ItemName);
			foreach (var item in items)
			{
				if (item is StackableData stackable && !stackable.IsFull())
				{
					stackableNew.CurrentAmount = stackable.AddAmount(stackableNew.CurrentAmount);
					var newItemGrids = this.GetModel<ContainerModel>().GetContainer(invName).FindGridsByItemData(item);
					System.Diagnostics.Debug.Assert(newItemGrids.Count > 0);
					// if (newItemGrids.Count > 0) throw new Exception("newItemGrids.Count > 0");
					this.SendEvent(new SigInvItemUpdatedEvent() { invName = invName, gridId = newItemGrids[0] });
					if (stackableNew.CurrentAmount <= 0)
						return true;
				}
			}
		}
		// 增加不可堆叠物品，或堆叠后剩余的物品
		var grids = this.GetModel<ContainerModel>().GetContainer(invName).AddItem(newItemData);
		if (grids != null && grids.Count > 0)
		{
			this.SendEvent(new SigInvItemAddedEvent() { invName = invName, itemData = newItemData, grids = grids });
			return true;
		}
		return false;
	}

	/// <summary>
	/// 尝试把正在移动的物品堆叠到这个格子上
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="gridId"></param>
	public void StackMovingItem(string invName, Vector2I gridId)
	{
		if (this.GetSystem<MovingItemService>().MovingItem == null)
			return;
		var itemData = FindItemDataByGrid(invName, gridId);
		if (itemData is not StackableData)
			return;
		if (itemData.ItemName == this.GetSystem<MovingItemService>().MovingItem.ItemName)
		{
			var amountLeft = ((StackableData)itemData).AddAmount(
				((StackableData)this.GetSystem<MovingItemService>().MovingItem).CurrentAmount);
			if (amountLeft > 0)
			{
				((StackableData)this.GetSystem<MovingItemService>().MovingItem).CurrentAmount = amountLeft;
			}
			else
			{
				this.GetSystem<MovingItemService>().ClearMovingItem();
			}
			this.SendEvent(new SigInvItemUpdatedEvent() { invName = invName, gridId = gridId });
		}
	}

	/// <summary>
	/// 尝试放置正在移动的物品到这个格子
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="gridId"></param>
	/// <returns></returns>
	public bool PlaceMovingItem(string invName, Vector2I gridId)
	{
		if (PlaceTo(invName, this.GetSystem<MovingItemService>().MovingItem, gridId))
		{
			this.GetSystem<MovingItemService>().ClearMovingItem();
			return true;
		}
		return false;
	}

	/// <summary>
	/// 使用物品（默认：鼠标右键点击格子）
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="gridId"></param>
	/// <returns></returns>
	public bool UseItem(string invName, Vector2I gridId)
	{
		var itemData = FindItemDataByGrid(invName, gridId);
		if (itemData == null)
			return false;
		if (itemData is EquipmentData)
		{
			if (this.GetSystem<EquipmentSystem>().TryEquip(itemData))
			{
				RemoveItemByData(invName, itemData);
				return true;
			}
		}
		else if (itemData is ConsumableData consumable)
		{
			if (consumable.Use())
			{
				RemoveItemByData(invName, itemData);
			}
			else
			{
				this.SendEvent(new SigInvItemUpdatedEvent() { invName = invName, gridId = gridId });
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// 分割物品
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="gridId"></param>
	/// <param name="offset"></param>
	/// <param name="baseSize"></param>
	/// <returns></returns>
	public ItemData SplitItem(string invName, Vector2I gridId, Vector2I offset, int baseSize)
	{
		var inv = this.GetModel<ContainerModel>().GetContainer(invName);
		if (inv != null)
		{
			var item = inv.FindItemDataByGrid(gridId);
			if (item != null && item is StackableData stackable && stackable.StackSize > 1 && stackable.CurrentAmount > 1)
			{
				int originAmount = stackable.CurrentAmount;
				int newAmount1 = originAmount / 2;
				int newAmount2 = originAmount - newAmount1;
				stackable.CurrentAmount = newAmount1;
				this.SendEvent(new SigInvItemUpdatedEvent() { invName = invName, gridId = gridId });

				var newItem = (StackableData)item.Duplicate();
				newItem.CurrentAmount = newAmount2;
				this.GetSystem<MovingItemService>().MoveItemByData(newItem, offset, baseSize);
				return newItem;
			}
		}
		return null;
	}

	/// <summary>
	/// 快速移动（默认：Shift + 鼠标右键）
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="gridId"></param>
	public void QuickMove(string invName, Vector2I gridId)
	{
		var targetInventories = this.GetModel<ContainerModel>().GetQuickMoveRelations(invName);
		var itemToMove = this.GetModel<ContainerModel>().GetContainer(invName).FindItemDataByGrid(gridId);
		if (targetInventories.Count == 0 || itemToMove == null)
			return;
		foreach (var targetContainer in targetInventories)
		{
			if (!this.GetModel<GBIS_Model>().OpenedContainers.Contains(targetContainer))
				continue;
			if (AddItem(targetContainer, itemToMove))
			{
				RemoveItemByData(invName, itemToMove);
				break;
			}
			else if (itemToMove is StackableData)
			{
				this.SendEvent(new SigInvItemUpdatedEvent() { invName = invName, gridId = gridId });
			}
		}
	}

	/// <summary>
	/// 增加背包间的快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="targetInvName"></param>
	public void AddQuickMoveRelation(string invName, string targetInvName)
	{
		this.GetModel<ContainerModel>().AddQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 删除背包间的快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="targetInvName"></param>
	public void RemoveQuickMoveRelation(string invName, string targetInvName)
	{
		this.GetModel<ContainerModel>().RemoveQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 删除背包中的物品，成功后触发 sig_inv_item_removed
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="itemData"></param>
	public void RemoveItemByData(string invName, ItemData itemData)
	{
		if (this.GetModel<ContainerModel>().GetContainer(invName).RemoveItem(itemData))
		{
			this.SendEvent(new SigInvItemRemovedEvent() { invName = invName, itemData = itemData });
		}
	}

	/// <summary>
	/// 只返回背包
	/// </summary>
	/// <param name="containerName"></param>
	/// <returns></returns>
	public ContainerData GetContainer(string containerName)
	{
		if (this.GetModel<GBIS_Model>().InventoryNames.Contains(containerName))
			return this.GetModel<ContainerModel>().GetContainer(containerName);
		return null;
	}
}
