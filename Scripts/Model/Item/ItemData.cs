using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 物品数据基类，不要直接继承这个类
/// </summary>
public partial class ItemData : Resource
{
	/// <summary>
	/// 调用后，将调用包含这个 data 的 view 的 QueueRedraw()。
	/// 场景：比如，强化装备后，修改了 shader 参数，但是不想重绘整个 Inventory，可以 emit 这个信号
	/// </summary>
	[Signal] public delegate void SigRefreshEventHandler();

	[ExportGroup("Common Settings")]
	/// <summary>
	/// 物品名称，需要唯一
	/// </summary>
	[Export] public string ItemName { get; set; } = "Item Name";
	/// <summary>
	/// 物品类型，值为"ANY"表示所有类型
	/// </summary>
	[Export] public string Type { get; set; } = "ANY";

	[ExportGroup("Display Settings")]
	/// <summary>
	/// 物品图标
	/// </summary>
	[Export] public Texture2D Icon { get; set; }
	/// <summary>
	/// 物品占的列数
	/// </summary>
	[Export] public int Columns { get; set; } = 1;
	/// <summary>
	/// 物品占的行数
	/// </summary>
	[Export] public int Rows { get; set; } = 1;
	/// <summary>
	/// view 上的材质，如果为空，则尝试获取 GBIS.material
	/// </summary>
	[Export] public ShaderMaterial Material { get; set; }
	/// <summary>
	/// 把 shader 需要修改的参数设置在这里
	/// </summary>
	[Export] public Dictionary<string, Variant> ShaderParams { get; set; } = new();

	/// <summary>
	/// 获取货品形状
	/// </summary>
	public Vector2I GetShape()
	{
		return new Vector2I(Columns, Rows);
	}

	/// <summary>
	/// 是否能丢弃
	/// </summary>
	public virtual bool CanDrop()
	{
		GD.PushWarning($"[Override this function] check if the item [{ItemName}] can drop");
		return true;
	}

	/// <summary>
	/// 丢弃物品时调用，需重写
	/// </summary>
	public virtual void Drop()
	{
		GD.PushWarning($"[Override this function] item [{ItemName}] dropped");
	}

	/// <summary>
	/// 物品是否能出售（是否贵重物品等）
	/// </summary>
	public virtual bool CanSell()
	{
		GD.PushWarning($"[Override this function] check if the item [{ItemName}] can be sell");
		return true;
	}

	/// <summary>
	/// 物品是否能购买（检查资源是否足够等）
	/// </summary>
	public virtual bool CanBuy()
	{
		GD.PushWarning($"[Override this function] check if the item [{ItemName}] can be bought");
		return true;
	}

	/// <summary>
	/// 购买后扣除资源
	/// </summary>
	public virtual void Cost()
	{
		GD.PushWarning($"[Override this function] [{ItemName}] cost resource");
	}

	/// <summary>
	/// 出售后增加资源
	/// </summary>
	public virtual void Sold()
	{
		GD.PushWarning($"[Override this function] [{ItemName}] add resource");
	}

	/// <summary>
	/// 购买并添加到背包
	/// </summary>
	public bool Buy()
	{
		if (!CanBuy())
			return false;
		foreach (var targetInv in GBIS_CSharp.Instance.InventoryNames)
		{
			if (GBIS_CSharp.Instance.InventoryService.AddItem(targetInv, this))
			{
				Cost();
				return true;
			}
		}
		return false;
	}
}
