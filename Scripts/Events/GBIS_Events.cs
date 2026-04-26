using Godot;
using Godot.Collections;
using System;

namespace GridBaseInventorySystem;

public sealed class GBIS_Events { }

#region 背包相关事件结构体
/// <summary>
/// 物品已添加事件
/// </summary>
public struct SigInvItemAddedEvent
{
	public string invName;
	public ItemData itemData;
	public Array<Vector2I> grids;
}

/// <summary>
/// 物品已移除事件
/// </summary>
public struct SigInvItemRemovedEvent
{
	public string invName;
	public ItemData itemData;
}

/// <summary>
/// 物品已更新事件
/// </summary>
public struct SigInvItemUpdatedEvent
{
	public string invName;
	public Vector2I gridId;
}

/// <summary>
/// 刷新所有背包事件
/// </summary>
public struct SigInvRefreshEvent
{

}
#endregion

#region 商店/装备槽相关事件结构体
/// <summary>
/// 刷新所有商店事件
/// </summary>
public struct SigShopRefreshEvent
{

}

/// <summary>
/// 刷新所有装备槽事件
/// </summary>
public struct SigSlotRefreshEvent
{

}

/// <summary>
/// 物品已装备事件
/// </summary>
public struct SigSlotItemEquippedEvent
{
	public string slotName;
	public ItemData itemData;
}

/// <summary>
/// 物品已脱下事件
/// </summary>
public struct SigSlotItemUnequippedEvent
{
	public string slotName;
	public ItemData itemData;
}
#endregion

#region 物品焦点相关事件结构体

/// <summary>
/// 物品获得焦点事件
/// </summary>
public struct SigItemFocusedEvent
{
	public ItemData itemData;
	public string containerName;
}

/// <summary>
/// 物品丢失焦点事件
/// </summary>
public struct SigItemFocusLostEvent
{
	public ItemData itemData;
}
#endregion