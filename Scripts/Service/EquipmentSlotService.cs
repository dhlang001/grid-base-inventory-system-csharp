using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 装备槽业务类
/// </summary>
public partial class EquipmentSlotService : Node
{
	/// <summary>
	/// 装备槽数据库引用
	/// </summary>
	protected EquipmentSlotRepository _equipmentSlotRepository = EquipmentSlotRepository.Instance;

	/// <summary>
	/// 保存所有装备槽数据
	/// </summary>
	public void SaveData()
	{
		_equipmentSlotRepository.SaveData();
	}

	/// <summary>
	/// 读取所有装备槽数据
	/// </summary>
	public void LoadData()
	{
		_equipmentSlotRepository.LoadData();
	}

	/// <summary>
	/// 获取指定名称的装备槽
	/// </summary>
	/// <param name="slotName"></param>
	/// <returns></returns>
	public EquipmentSlotData GetSlot(string slotName)
	{
		return _equipmentSlotRepository.GetSlot(slotName);
	}

	/// <summary>
	/// 注册装备槽，如果重名，则检测是否和已有的数据相符
	/// 注意：如果装备槽不显示，大概率是注册返回失败了，请检查配置
	/// </summary>
	/// <param name="slotName"></param>
	/// <param name="avilableTypes"></param>
	/// <returns></returns>
	public bool RegistSlot(string slotName, Array<string> avilableTypes)
	{
		var slotData = _equipmentSlotRepository.GetSlot(slotName);
		if (slotData != null)
		{
			bool isSameAvilableTypes = avilableTypes.Count == slotData.AvilableTypes.Count;
			if (isSameAvilableTypes)
			{
				for (int i = 0; i < avilableTypes.Count; i++)
				{
					isSameAvilableTypes = avilableTypes[i] == slotData.AvilableTypes[i];
					if (!isSameAvilableTypes)
						break;
				}
			}
			return isSameAvilableTypes;
		}
		else
		{
			return _equipmentSlotRepository.AddSlot(slotName, avilableTypes);
		}
	}

	/// <summary>
	/// 尝试穿戴装备，如果成功，发射信号 sig_slot_item_equipped
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public bool TryEquip(ItemData itemData)
	{
		if (itemData == null)
			return false;
		var slot = _equipmentSlotRepository.TryEquip(itemData);
		if (slot != null)
		{
			GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigSlotItemEquipped, slot.SlotName, itemData);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 尝试装备正在移动的物品，返回是否成功
	/// </summary>
	/// <param name="slotName"></param>
	/// <returns></returns>
	public bool EquipMovingItem(string slotName)
	{
		if (EquipTo(slotName, GBIS_CSharp.Instance.MovingItemService.MovingItem))
		{
			GBIS_CSharp.Instance.MovingItemService.ClearMovingItem();
			return true;
		}
		return false;
	}

	/// <summary>
	/// 装备物品到指定的装备槽，成功后发射信号 sig_slot_item_equipped
	/// </summary>
	/// <param name="slotName"></param>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public bool EquipTo(string slotName, ItemData itemData)
	{
		if (_equipmentSlotRepository.GetSlot(slotName).Equip(itemData))
		{
			GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigSlotItemEquipped, slotName, itemData);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 脱掉装备，成功后发射信号 sig_slot_item_unequipped
	/// </summary>
	/// <param name="slotName"></param>
	/// <returns></returns>
	public ItemData Unequip(string slotName)
	{
		var openedContainers = new Array<string>(GBIS_CSharp.Instance.OpenedContainers);
		// reverse iteration
		for (int i = openedContainers.Count - 1; i >= 0; i--)
		{
			var currentInventory = openedContainers[i];
			if (!GBIS_CSharp.Instance.InventoryNames.Contains(currentInventory))
				continue;
			var itemData = GetSlot(slotName).EquippedItem;
			if (itemData != null && GBIS_CSharp.Instance.InventoryService.AddItem(currentInventory, itemData))
			{
				_equipmentSlotRepository.GetSlot(slotName).Unequip();
				GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigSlotItemUnequipped, slotName, itemData);
				return itemData;
			}
		}
		return null;
	}

	/// <summary>
	/// 移动正在装备的物品，成功后发射信号 sig_slot_item_unequipped
	/// </summary>
	/// <param name="slotName"></param>
	/// <param name="baseSize"></param>
	public void MoveItem(string slotName, int baseSize)
	{
		if (GBIS_CSharp.Instance.MovingItemService.MovingItem != null)
		{
			GD.PushError("Already had moving item.");
			return;
		}
		var itemData = GetSlot(slotName).EquippedItem;
		if (itemData != null)
		{
			if (_equipmentSlotRepository.GetSlot(slotName).Unequip() != null)
			{
				GBIS_CSharp.Instance.MovingItemService.MoveItemByData(itemData, Vector2I.Zero, baseSize);
				GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigSlotItemUnequipped, slotName, itemData);
			}
		}
	}
}
