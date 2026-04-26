using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 装备槽数据库，管理 EquipmentSlotData 的存取
/// </summary>
public partial class EquipmentSlotRepository : Resource
{

	private static EquipmentSlotRepository _instance;
	/// <summary>
	/// 单例
	/// </summary>
	public static EquipmentSlotRepository Instance => _instance ??= new EquipmentSlotRepository();

	/// <summary>
	/// 系统中所有的装备槽
	/// </summary>
	[Export] public Godot.Collections.Dictionary<string, EquipmentSlotData> SlotDataMap { get; set; } = new();

	/// <summary>
	/// 保存所有装备槽
	/// </summary>
	public void SaveData()
	{
		ResourceSaver.Save(this, GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_EquipmentSlotData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName);
	}

	/// <summary>
	/// 读取所有装备槽，会重新穿戴所有装备
	/// </summary>
	public void LoadData()
	{
		foreach (var slotName in SlotDataMap.Keys)
		{
			var itemData = SlotDataMap[slotName].EquippedItem as EquipmentData;
			if (itemData != null)
			{
				itemData.Unequipped(slotName);
			}
		}

		var savedRepository = GD.Load<EquipmentSlotRepository>(GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_EquipmentSlotData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName);
		if (savedRepository == null) return;
		foreach (var slotName in savedRepository.SlotDataMap.Keys)
		{
			SlotDataMap[slotName] = (EquipmentSlotData)savedRepository.SlotDataMap[slotName].Duplicate(true);
			EquipmentData itemData = SlotDataMap[slotName].EquippedItem as EquipmentData;
			if (itemData != null)
			{
				itemData.Equipped(slotName);
			}
		}
	}

	/// <summary>
	/// 获取指定装备槽的数据类
	/// </summary>
	/// <param name="slotName"></param>
	/// <returns></returns>
	public EquipmentSlotData GetSlot(string slotName)
	{
		return SlotDataMap.TryGetValue(slotName, out var data) ? data : null;
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
			SlotDataMap[slotName] = newSlot;
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
		foreach (var slot in SlotDataMap.Values)
		{
			if (GameArchitecture.Interface.GetModel<GBIS_Model>().OpenedEquipmentSlots.Contains(slot.SlotName) && slot.Equip(itemData))
			{
				return slot;
			}
		}
		return null;
	}
}
