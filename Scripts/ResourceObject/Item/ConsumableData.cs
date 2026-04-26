using Godot;

namespace GridBaseInventorySystem;

/// <summary>
/// 消耗品数据基类，你的消耗品数据类应该继承此类
/// </summary>
public partial class ConsumableData : StackableData
{
	/// <summary>
	/// 当数量为0时，是否摧毁物品
	/// </summary>
	[Export] public bool DestroyIfEmpty { get; set; } = true;

	/// <summary>
	/// 物品被使用时调用
	/// </summary>
	public bool Use()
	{
		if (CurrentAmount > 0)
		{
			int consumedAmount = Consume();
			if (consumedAmount > 0)
			{
				CurrentAmount -= consumedAmount;
				if (CurrentAmount <= 0)
					return DestroyIfEmpty;
			}
		}
		return false;
	}

	/// <summary>
	/// 消耗方法，需重写，返回消耗数量（>=0）
	/// </summary>
	public virtual int Consume()
	{
		GD.PushWarning($"[Override this function] consumable item [{ItemName}] has been consumed");
		return 1;
	}
}
