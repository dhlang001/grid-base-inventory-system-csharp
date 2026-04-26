using Godot;
using Godot.Collections;

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
					var newItemGrids = _containerRepository.GetContainer(invName).FindGridsByItemData(item);
					System.Diagnostics.Debug.Assert(newItemGrids.Count > 0);
					GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemUpdated, invName, newItemGrids[0]);
					if (stackableNew.CurrentAmount <= 0)
						return true;
				}
			}
		}
		// 增加不可堆叠物品，或堆叠后剩余的物品
		var grids = _containerRepository.GetContainer(invName).AddItem(newItemData);
		if (grids != null && grids.Count > 0)
		{
			GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemAdded, invName, newItemData, grids);
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
		if (GBIS_CSharp.Instance.MovingItemService.MovingItem == null)
			return;
		var itemData = FindItemDataByGrid(invName, gridId);
		if (itemData is not StackableData)
			return;
		if (itemData.ItemName == GBIS_CSharp.Instance.MovingItemService.MovingItem.ItemName)
		{
			var amountLeft = ((StackableData)itemData).AddAmount(
				((StackableData)GBIS_CSharp.Instance.MovingItemService.MovingItem).CurrentAmount);
			if (amountLeft > 0)
			{
				((StackableData)GBIS_CSharp.Instance.MovingItemService.MovingItem).CurrentAmount = amountLeft;
			}
			else
			{
				GBIS_CSharp.Instance.MovingItemService.ClearMovingItem();
			}
			GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemUpdated, invName, gridId);
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
		if (PlaceTo(invName, GBIS_CSharp.Instance.MovingItemService.MovingItem, gridId))
		{
			GBIS_CSharp.Instance.MovingItemService.ClearMovingItem();
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
			if (GBIS_CSharp.Instance.EquipmentSlotService.TryEquip(itemData))
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
				GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemUpdated, invName, gridId);
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
		var inv = _containerRepository.GetContainer(invName);
		if (inv != null)
		{
			var item = inv.FindItemDataByGrid(gridId);
			if (item != null && item is StackableData stackable && stackable.StackSize > 1 && stackable.CurrentAmount > 1)
			{
				int originAmount = stackable.CurrentAmount;
				int newAmount1 = originAmount / 2;
				int newAmount2 = originAmount - newAmount1;
				stackable.CurrentAmount = newAmount1;
				GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemUpdated, invName, gridId);

				var newItem = (StackableData)item.Duplicate();
				newItem.CurrentAmount = newAmount2;
				GBIS_CSharp.Instance.MovingItemService.MoveItemByData(newItem, offset, baseSize);
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
		var targetInventories = _containerRepository.GetQuickMoveRelations(invName);
		var itemToMove = _containerRepository.GetContainer(invName).FindItemDataByGrid(gridId);
		if (targetInventories.Count == 0 || itemToMove == null)
			return;
		foreach (var targetContainer in targetInventories)
		{
			if (!GBIS_CSharp.Instance.OpenedContainers.Contains(targetContainer))
				continue;
			if (AddItem(targetContainer, itemToMove))
			{
				RemoveItemByData(invName, itemToMove);
				break;
			}
			else if (itemToMove is StackableData)
			{
				GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemUpdated, invName, gridId);
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
		_containerRepository.AddQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 删除背包间的快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="targetInvName"></param>
	public void RemoveQuickMoveRelation(string invName, string targetInvName)
	{
		_containerRepository.RemoveQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 删除背包中的物品，成功后触发 sig_inv_item_removed
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="itemData"></param>
	public void RemoveItemByData(string invName, ItemData itemData)
	{
		if (_containerRepository.GetContainer(invName).RemoveItem(itemData))
		{
			GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemRemoved, invName, itemData);
		}
	}

	/// <summary>
	/// 只返回背包
	/// </summary>
	/// <param name="containerName"></param>
	/// <returns></returns>
	public ContainerData GetContainer(string containerName)
	{
		if (GBIS_CSharp.Instance.InventoryNames.Contains(containerName))
			return _containerRepository.GetContainer(containerName);
		return null;
	}
}
