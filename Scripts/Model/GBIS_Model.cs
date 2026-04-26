using Godot;
using Godot.Collections;
using QFramework;
using System;

namespace GridBaseInventorySystem;

public class GBIS_Model : AbstractModel
{

	public Node GBIS_BaseNode { get; set; }

	#region 配置字段

	/// <summary>
	/// 物品的 Material，如果不为空，则 ItemView 在创建时会给物品附加这个材质
	/// </summary>
	public ShaderMaterial ItemMaterial { get; set; }

	/// <summary>
	/// 所有背包的name
	/// </summary>
	public Array<string> InventoryNames { get; set; } = new();
	/// <summary>
	/// 所有商店的name
	/// </summary>
	public Array<string> ShopNames { get; set; } = new();

	/// <summary>
	/// 当前打开的container（包含背包和商店）
	/// </summary>
	public Array<string> OpenedContainers { get; set; } = new();
	/// <summary>
	/// 当前打开的装备槽
	/// </summary>
	public Array<string> OpenedEquipmentSlots { get; set; } = new();

	/// <summary>
	/// 当前保存路径
	/// </summary>
	public string CurrentSavePath { get; set; } = GBIS_Const.DefaultSaveFolder;
	/// <summary>
	/// 当前存档名
	/// </summary>
	public string CurrentSaveName { get; set; } = "default.tres";

	/// <summary>
	/// 点击物品
	/// </summary>
	public string InputClick { get; set; } = "inv_click";
	/// <summary>
	/// 快速移动
	/// </summary>
	public string InputQuickMove { get; set; } = "inv_quick_move";
	/// <summary>
	/// 使用物品
	/// </summary>
	public string InputUse { get; set; } = "inv_use";
	/// <summary>
	/// 分割物品
	/// </summary>
	public string InputSplit { get; set; } = "inv_split";

	#endregion
	protected override void OnInit()
	{

	}
}
