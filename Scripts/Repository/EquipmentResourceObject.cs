using Godot;
using Godot.Collections;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 装备槽数据库，管理 EquipmentSlotData 的存取
/// </summary>
public partial class EquipmentResourceObject : Resource
{

	/// <summary>
	/// 系统中所有的装备槽
	/// </summary>
	[Export] public Godot.Collections.Dictionary<string, EquipmentSlotData> SlotDataMap { get; set; } = new();

}
