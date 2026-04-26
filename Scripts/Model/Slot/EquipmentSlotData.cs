using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 装备槽数据类，管理穿脱装备
/// </summary>
public partial class EquipmentSlotData : Resource
{
	/// <summary>
	/// 已装备的物品，未装备时null，可用于检测是否有装备
	/// </summary>
	[Export] public ItemData EquippedItem { get; set; }
	/// <summary>
	/// 允许装备的物品类型，对应ItemData.type
	/// </summary>
	[Export] public Array<string> AvilableTypes { get; set; } = new();
	/// <summary>
	/// 装备槽的名字
	/// </summary>
	[Export] public string SlotName { get; set; }

	public EquipmentSlotData() { }

	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="slotName"></param>
	/// <param name="avilableTypes"></param>
	public void Init(string slotName, Array<string> avilableTypes)
	{
		SlotName = slotName;
		AvilableTypes = avilableTypes;
	}

	/// <summary>
	/// 装备物品
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public bool Equip(ItemData itemData)
	{
		if (EquippedItem == null)
		{
			if (IsItemAvilable(itemData))
			{
				EquippedItem = itemData;
				((EquipmentData)EquippedItem).Equipped(SlotName);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 脱掉装备，返回被脱掉的物品
	/// </summary>
	/// <returns></returns>
	public ItemData Unequip()
	{
		if (EquippedItem == null)
			return null;
		var ret = EquippedItem;
		((EquipmentData)ret).Unequipped(SlotName);
		EquippedItem = null;
		return ret;
	}

	/// <summary>
	/// 检查是否可装备这个物品
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public bool IsItemAvilable(ItemData itemData)
	{
		if (AvilableTypes.Contains("ANY") || AvilableTypes.Contains(itemData.Type))
			return itemData is EquipmentData eqData && eqData.TestNeed(SlotName);
		return false;
	}
}
