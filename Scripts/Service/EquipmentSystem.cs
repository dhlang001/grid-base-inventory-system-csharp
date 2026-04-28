using Godot;
using Godot.Collections;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 装备槽业务类
/// </summary>
public partial class EquipmentSystem : AbstractSystem
{

	protected override void OnInit()
	{

	}

	/// <summary>
	/// 保存所有装备槽数据
	/// 注：可能需要重构到存档系统统一管理
	/// </summary>
	public void SaveData()
	{
		if (this.GetModel<EquipmentModel>().EquipmentResourceObject == null)
		{
			this.GetModel<EquipmentModel>().EquipmentResourceObject = new EquipmentResourceObject();
		}
		this.GetModel<EquipmentModel>().EquipmentResourceObject.SlotDataMap = this.GetModel<EquipmentModel>().EquipmentResourceObject.SlotDataMap;
		ResourceSaver.Save(this.GetModel<EquipmentModel>().EquipmentResourceObject, GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_EquipmentSlotData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName);
	}

	/// <summary>
	/// 读取所有装备槽，会重新穿戴所有装备
	/// 注：可能需要重构到存档系统统一管理
	/// </summary>
	public void LoadData()
	{
		foreach (var slotName in this.GetModel<EquipmentModel>().EquipmentResourceObject.SlotDataMap.Keys)
		{
			var itemData = this.GetModel<EquipmentModel>().EquipmentResourceObject.SlotDataMap[slotName].EquippedItem as EquipmentData;
			if (itemData != null)
			{
				itemData.Unequipped(slotName);
			}
		}

		var saveRepository = GD.Load<EquipmentResourceObject>(GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_EquipmentSlotData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName);
		if (saveRepository == null) return;
		this.GetModel<EquipmentModel>().EquipmentResourceObject = saveRepository.DuplicateDeep() as EquipmentResourceObject;
	}

	/// <summary>
	/// 获取指定名称的装备槽
	/// </summary>
	/// <param name="slotName"></param>
	/// <returns></returns>
	public EquipmentSlotData GetSlot(string slotName)
	{
		return this.GetModel<EquipmentModel>().GetSlot(slotName);
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
		var slotData = this.GetModel<EquipmentModel>().GetSlot(slotName);
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
			return this.GetModel<EquipmentModel>().AddSlot(slotName, avilableTypes);
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
		var slot = this.GetModel<EquipmentModel>().TryEquip(itemData);
		if (slot != null)
		{
			this.SendEvent<SigSlotItemEquippedEvent>(new SigSlotItemEquippedEvent() { slotName = slot.SlotName, itemData = itemData });
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
		if (EquipTo(slotName, this.GetSystem<MovingItemService>().MovingItem))
		{
			this.GetSystem<MovingItemService>().ClearMovingItem();
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
		if (this.GetModel<EquipmentModel>().GetSlot(slotName).Equip(itemData))
		{
			this.SendEvent<SigSlotItemEquippedEvent>(new SigSlotItemEquippedEvent() { slotName = slotName, itemData = itemData });
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
		var openedContainers = new Array<string>(this.GetModel<GBIS_Model>().OpenedContainers);
		// reverse iteration
		for (int i = openedContainers.Count - 1; i >= 0; i--)
		{
			var currentInventory = openedContainers[i];
			if (!this.GetModel<GBIS_Model>().InventoryNames.Contains(currentInventory))
				continue;
			var itemData = GetSlot(slotName).EquippedItem;
			if (itemData != null && this.GetSystem<InventoryService>().AddItem(currentInventory, itemData))
			{
				this.GetModel<EquipmentModel>().GetSlot(slotName).Unequip();
				this.SendEvent<SigSlotItemEquippedEvent>(new SigSlotItemEquippedEvent() { slotName = slotName, itemData = itemData });
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
		if (this.GetSystem<MovingItemService>().MovingItem != null)
		{
			GD.PushError("Already had moving item.");
			return;
		}
		var itemData = GetSlot(slotName).EquippedItem;
		if (itemData != null)
		{
			if (this.GetModel<EquipmentModel>().GetSlot(slotName).Unequip() != null)
			{
				this.GetSystem<MovingItemService>().MoveItemByData(itemData, Vector2I.Zero, baseSize);
				this.SendEvent<SigSlotItemEquippedEvent>(new SigSlotItemEquippedEvent() { slotName = slotName, itemData = itemData });
			}
		}
	}
}
