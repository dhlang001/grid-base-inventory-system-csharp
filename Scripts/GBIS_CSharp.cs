using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 网格背包系统全局入口，Autoload 单例
/// ========== 重要 ==========
/// 全局名称必须配置为：GBIS
/// ========== 重要 ==========
/// </summary>
[GlobalClass]
public partial class GBIS_CSharp : Node
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

	#endregion

	#region 信号

	/// <summary>
	/// 物品已添加
	/// </summary>
	[Signal] public delegate void SigInvItemAddedEventHandler(string invName, ItemData itemData, Array<Vector2I> grids);
	/// <summary>
	/// 物品已移除
	/// </summary>
	[Signal] public delegate void SigInvItemRemovedEventHandler(string invName, ItemData itemData);
	/// <summary>
	/// 物品已更新
	/// </summary>
	[Signal] public delegate void SigInvItemUpdatedEventHandler(string invName, Vector2I gridId);
	/// <summary>
	/// 刷新所有背包
	/// </summary>
	[Signal] public delegate void SigInvRefreshEventHandler();
	/// <summary>
	/// 刷新所有商店
	/// </summary>
	[Signal] public delegate void SigShopRefreshEventHandler();
	/// <summary>
	/// 刷新所有装备槽
	/// </summary>
	[Signal] public delegate void SigSlotRefreshEventHandler();
	/// <summary>
	/// 物品已装备
	/// </summary>
	[Signal] public delegate void SigSlotItemEquippedEventHandler(string slotName, ItemData itemData);
	/// <summary>
	/// 物品已脱下
	/// </summary>
	[Signal] public delegate void SigSlotItemUnequippedEventHandler(string slotName, ItemData itemData);
	/// <summary>
	/// 焦点物品：监听这个信号以处理信息显示
	/// </summary>
	[Signal] public delegate void SigItemFocusedEventHandler(ItemData itemData, string containerName);
	/// <summary>
	/// 物品丢失焦点：监听这个信号以清除物品信息显示
	/// </summary>
	[Signal] public delegate void SigItemFocusLostEventHandler(ItemData itemData);

	#endregion

	#region 静态实例

	/// <summary>
	/// 全局单例访问
	/// </summary>
	public static GBIS_CSharp Instance { get; private set; }

	#endregion

	#region Service 字段

	/// <summary>
	/// 背包业务类全局引用，如有需要可以使用，不要自己new
	/// </summary>
	public InventoryService InventoryService { get; set; }
	/// <summary>
	/// 商店业务类全局引用，如有需要可以使用，不要自己new
	/// </summary>
	public ShopService ShopService { get; set; }
	/// <summary>
	/// 装备槽业务类全局引用，如有需要可以使用，不要自己new
	/// </summary>
	public EquipmentSlotService EquipmentSlotService { get; set; }
	/// <summary>
	/// 移动物品业务类全局引用，如有需要可以使用，不要自己new
	/// </summary>
	public MovingItemService MovingItemService { get; set; }
	/// <summary>
	/// 物品焦点业务类全局引用，如有需要可以使用，不要自己new
	/// </summary>
	public ItemFocusService ItemFocusService { get; set; }

	#endregion

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
	public string CurrentSavePath { get; set; } = DefaultSaveFolder;
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

	#region 生命周期

	public override void _Ready()
	{
		Instance = this;
		InventoryService = new InventoryService();
		ShopService = new ShopService();
		EquipmentSlotService = new EquipmentSlotService();
		MovingItemService = new MovingItemService();
		ItemFocusService = new ItemFocusService();
	}

	#endregion

	#region 公开 API

	/// <summary>
	/// 保存背包和装备槽
	/// </summary>
	public void SaveData()
	{
		InventoryService.SaveData();
		EquipmentSlotService.SaveData();
	}

	/// <summary>
	/// 读取背包和装备槽
	/// </summary>
	public async void LoadData()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		InventoryService.LoadData();
		EquipmentSlotService.LoadData();
		EmitSignal(SignalName.SigInvRefresh);
		EmitSignal(SignalName.SigSlotRefresh);
		EmitSignal(SignalName.SigShopRefresh);
	}

	/// <summary>
	/// 获取场景树的根（主要在Service中使用，因为Service没有加入场景树，所以没有 GetTree()）
	/// </summary>
	public Node GetRoot()
	{
		return GetTree().Root;
	}

	/// <summary>
	/// 向背包添加物品
	/// </summary>
	public bool AddItem(string invName, ItemData itemData)
	{
		return InventoryService.AddItem(invName, itemData);
	}

	/// <summary>
	/// 增加背包间的快速移动关系
	/// </summary>
	public void AddQuickMoveRelation(string invName, string targetInvName)
	{
		InventoryService.AddQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 删除背包间的快速移动关系
	/// </summary>
	public void RemoveQuickMoveRelation(string invName, string targetInvName)
	{
		InventoryService.RemoveQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 是否有正在移动的物品
	/// </summary>
	public bool HasMovingItem()
	{
		return MovingItemService.MovingItem != null;
	}

	#endregion
}
