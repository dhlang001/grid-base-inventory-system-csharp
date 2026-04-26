using Godot;

namespace GridBaseInventorySystem;

/// <summary>
/// 装备数据基类，你的装备数据类应该继承此类
/// </summary>
public partial class EquipmentData : ItemData
{
	/// <summary>
	/// 检测装备是否可用，需重写
	/// </summary>
	public virtual bool TestNeed(string slotName)
	{
		GD.PushWarning($"[Override this function] [{ItemName}] test passed.");
		return true;
	}

	/// <summary>
	/// 装备时调用，需重写；也可以使用 GBIS.SigSlotItemEquipped 信号行处理
	/// </summary>
	public virtual void Equipped(string slotName)
	{
		GD.PushWarning($"[Override this function] equipped item [{ItemName}] at slot [{slotName}]");
	}

	/// <summary>
	/// 脱装备时调用，需重写；也可以用 GBIS.SigSlotItemUnequipped 信号进行处理
	/// </summary>
	public virtual void Unequipped(string slotName)
	{
		GD.PushWarning($"[Override this function] unequipped item [{ItemName}] at slot [{slotName}]");
	}
}
