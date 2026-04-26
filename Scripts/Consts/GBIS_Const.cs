using Godot;
using System;

namespace GridBaseInventorySystem;

/// <summary>
/// 网格背包系统常量
/// </summary>
public static class GBIS_Const
{

	#region 常量

	/// <summary>
	/// 默认角色
	/// </summary>
	public const string DefaultPlayer = "player_1";
	/// <summary>
	/// 默认背包名称
	/// </summary>
	public const string DefaultInventoryName = "Inventory";
	/// <summary>
	/// 默认商店名称
	/// </summary>
	public const string DefaultShopName = "Shop";
	/// <summary>
	/// 默认装备槽名称
	/// </summary>
	public const string DefaultSlotName = "Equipment Slot";
	/// <summary>
	/// 默认保存路径
	/// </summary>
	public const string DefaultSaveFolder = "res://Plugins/GBIS/saves/";

	/// <summary>
	/// ContainerData保存时的前缀
	/// </summary>
	public const string Prefix_ContainerData = "container_";
	/// <summary>
	/// EquipmentSlotData保存时的前缀
	/// </summary>
	public const string Prefix_EquipmentSlotData = "equipment_slot_";

	#endregion

}
