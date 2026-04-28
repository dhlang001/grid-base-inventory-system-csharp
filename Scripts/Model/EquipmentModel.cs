using Godot;
using Godot.Collections;
using QFramework;
using System;

namespace GridBaseInventorySystem;

public class EquipmentModel : AbstractModel
{

	/// <summary>
	/// Equipment存储对象实例
	/// </summary>
	public EquipmentResourceObject EquipmentResourceObject { get; set; }

	protected override void OnInit()
	{
		EquipmentResourceObject ??= new EquipmentResourceObject();
	}

	/// <summary>
	/// 获取指定装备槽的数据类
	/// </summary>
	/// <param name="slotName"></param>
	/// <returns></returns>
	public EquipmentSlotData GetSlot(string slotName)
	{
		return EquipmentResourceObject.SlotDataMap.TryGetValue(slotName, out var data) ? data : null;
	}

	/// <summary>
	/// 增加一个装备槽
	/// </summary>
	/// <param name="slotName"></param>
	/// <param name="avilableTypes"></param>
	/// <returns></returns>
	public bool AddSlot(string slotName, Array<string> avilableTypes)
	{
		var slot = GetSlot(slotName);
		if (slot == null)
		{
			var newSlot = new EquipmentSlotData();
			newSlot.Init(slotName, avilableTypes);
			EquipmentResourceObject.SlotDataMap[slotName] = newSlot;
			return true;
		}
		return false;
	}

	/// <summary>
	/// 尝试装备一件物品，如果装备成功，返回装备上这个物品的装备槽
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public EquipmentSlotData TryEquip(ItemData itemData)
	{
		foreach (var slot in EquipmentResourceObject.SlotDataMap.Values)
		{
			if (GameArchitecture.Interface.GetModel<GBIS_Model>().OpenedEquipmentSlots.Contains(slot.SlotName) && slot.Equip(itemData))
			{
				return slot;
			}
		}
		return null;
	}


}
