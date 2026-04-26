using Godot;

namespace GridBaseInventorySystem;

/// <summary>
/// 可堆叠物品数据基类，你的可堆叠物品数据类应继承此类（如：可堆叠的宝石）。注意：消耗品应继承 ConsumableData
/// </summary>
public partial class StackableData : ItemData
{
	/// <summary>
	/// 最大堆叠数量
	/// </summary>
	[Export] public int StackSize { get; set; } = 2;
	/// <summary>
	/// 当前数量
	/// </summary>
	[Export] public int CurrentAmount { get; set; } = 1;

	/// <summary>
	/// 是否堆叠满了
	/// </summary>
	public bool IsFull()
	{
		return CurrentAmount >= StackSize;
	}

	/// <summary>
	/// 增加堆叠数量，返回剩余数量
	/// </summary>
	public int AddAmount(int amount)
	{
		if (IsFull())
			return amount;
		int amountLeft = StackSize - CurrentAmount;
		if (amountLeft < amount)
		{
			CurrentAmount = StackSize;
			return amount - amountLeft;
		}
		CurrentAmount += amount;
		return 0;
	}
}
