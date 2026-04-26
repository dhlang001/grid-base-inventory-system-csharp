using Godot;
using System;

namespace GridBaseInventorySystem;

public sealed class GBIS_Enums { }

/// <summary>
/// 装备槽的绘制状态：正常、可用、不可用
/// </summary>
public enum SlotState
{
	/// <summary>
	/// 正常
	/// </summary>
	Normal,
	/// <summary>
	/// 可用
	/// </summary>
	Avilable,
	/// <summary>
	/// 不可用
	/// </summary>
	Invilable
}

/// <summary>
/// 格子的绘制状态：空、占用、冲突、可用
/// </summary>
public enum GridState
{
	/// <summary>
	/// 空
	/// </summary>
	Empty,
	/// <summary>
	/// 占用
	/// </summary>
	Taken,
	/// <summary>
	/// 冲突
	/// </summary>
	Conflict,
	/// <summary>
	/// 可用
	/// </summary>
	Avilable
}