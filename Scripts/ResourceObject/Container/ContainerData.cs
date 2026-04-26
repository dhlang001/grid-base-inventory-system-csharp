using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 库存数据类，管理物品在网格中的存储和操作
/// </summary>
public partial class ContainerData : Resource
{
	/// <summary>
	/// 库存列数
	/// </summary>
	[Export] public int Columns { get; set; } = 2;
	/// <summary>
	/// 库存行数
	/// </summary>
	[Export] public int Rows { get; set; } = 2;
	/// <summary>
	/// 库存名称
	/// </summary>
	[Export] public string ContainerName { get; set; }
	/// <summary>
	/// 允许存放的物品类型列表
	/// </summary>
	[Export] public Array<string> AvilableTypes { get; set; } = new();
	/// <summary>
	/// 当前存放的物品数据列表
	/// </summary>
	[Export] public Array<ItemData> Items { get; set; } = new();
	/// <summary>
	/// 物品到占据网格的映射(Array[grid_id: Vector2i])
	/// </summary>
	[Export] public Godot.Collections.Dictionary<ItemData, Array<Vector2I>> ItemGridsMap { get; set; } = new();
	/// <summary>
	/// 格子到物品的映射
	/// </summary>
	[Export] public Godot.Collections.Dictionary<Vector2I, ItemData> GridItemMap { get; set; } = new();

	public ContainerData() { }

	/// <summary>
	/// 初始化容器（Resource 子类必须有无参构造函数）
	/// </summary>
	public void Init(string containerName, int columns, int rows, Array<string> avilableTypes)
	{
		ContainerName = containerName;
		AvilableTypes = avilableTypes;
		Columns = columns;
		Rows = rows;
		for (int row = 0; row < rows; row++)
		{
			for (int col = 0; col < columns; col++)
			{
				GridItemMap[new Vector2I(col, row)] = null;
			}
		}
	}

	/// <summary>
	/// 清空重启
	/// </summary>
	public void Clear()
	{
		Items.Clear();
		ItemGridsMap.Clear();
		for (int row = 0; row < Rows; row++)
		{
			for (int col = 0; col < Columns; col++)
			{
				GridItemMap[new Vector2I(col, row)] = null;
			}
		}
	}

	/// <summary>
	/// 深度复制当前库存数据
	/// </summary>
	/// <returns></returns>
	public ContainerData DeepDuplicate()
	{
		var ret = new ContainerData();
		ret.Init(ContainerName, Columns, Rows, AvilableTypes);
		var keys = new Godot.Collections.Array<ItemData>(ItemGridsMap.Keys);
		foreach (var itemData in keys)
		{
			var duplicatedItem = (ItemData)itemData.Duplicate();
			var grids = new Array<Vector2I>(ItemGridsMap[itemData]);
			ret.ItemGridsMap[duplicatedItem] = grids;
			ret.Items.Add(duplicatedItem);
		}
		foreach (var item in ret.Items)
		{
			var grids = ret.ItemGridsMap[item];
			foreach (var grid in grids)
			{
				ret.GridItemMap[grid] = item;
			}
		}
		return ret;
	}

	/// <summary>
	/// 添加物品到库存，返回物品占用的网格坐标列表
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public Array<Vector2I> AddItem(ItemData itemData)
	{
		if (!IsItemAvilable(itemData))
			return new Array<Vector2I>();
		var grids = FindFirstAvailbleGrids(itemData);
		AddItemToGrids(itemData, grids);
		return grids;
	}

	/// <summary>
	/// 从库存中移除物品，返回是否移除成功
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool RemoveItem(ItemData item)
	{
		if (!Items.Contains(item))
			return false;
		var grids = ItemGridsMap[item];
		foreach (var grid in grids)
		{
			GridItemMap[grid] = null;
		}
		Items.Remove(item);
		ItemGridsMap.Remove(item);
		return true;
	}

	/// <summary>
	/// 检查物品是否可以被放入当前库存
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public bool IsItemAvilable(ItemData itemData)
	{
		return AvilableTypes.Contains("ANY") || AvilableTypes.Contains(itemData.Type);
	}

	/// <summary>
	/// 根据物品数据查找其占用的网格坐标列表
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public Array<Vector2I> FindGridsByItemData(ItemData itemData)
	{
		return ItemGridsMap.TryGetValue(itemData, out var grids) ? grids : new Array<Vector2I>();
	}

	/// <summary>
	/// 检查库存中是否包含指定物品
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool HasItem(ItemData item)
	{
		return Items.Contains(item);
	}

	/// <summary>
	/// 根据网格坐标查找对应的物品数据
	/// </summary>
	/// <param name="gridId"></param>
	/// <returns></returns>
	public ItemData FindItemDataByGrid(Vector2I gridId)
	{
		return GridItemMap.TryGetValue(gridId, out var item) ? item : null;
	}

	/// <summary>
	/// 尝试将物品添加到指定网格位置，返回实际占用的网格坐标列表
	/// </summary>
	/// <param name="itemData"></param>
	/// <param name="gridId"></param>
	/// <returns></returns>
	public Array<Vector2I> TryAddToGrid(ItemData itemData, Vector2I gridId)
	{
		if (!IsItemAvilable(itemData))
			return new Array<Vector2I>();
		var grids = TryGetEmptyGridsByShape(gridId, itemData.GetShape());
		AddItemToGrids(itemData, grids);
		return grids;
	}

	/// <summary>
	/// 根据物品名称查找所有匹配的物品数据
	/// </summary>
	/// <param name="itemName"></param>
	/// <returns></returns>
	public Array<ItemData> FindItemDataByItemName(string itemName)
	{
		var ret = new Array<ItemData>();
		foreach (var item in Items)
		{
			if (item.ItemName == itemName)
				ret.Add(item);
		}
		return ret;
	}

	/// <summary>
	/// 将物品添加到指定网格位置，返回是否添加成功
	/// </summary>
	/// <param name="itemData"></param>
	/// <param name="grids"></param>
	/// <returns></returns>
	private bool AddItemToGrids(ItemData itemData, Array<Vector2I> grids)
	{
		if (grids == null || grids.Count == 0)
			return false;
		Items.Add(itemData);
		ItemGridsMap[itemData] = grids;
		foreach (var grid in grids)
		{
			GridItemMap[grid] = itemData;
		}
		return true;
	}

	/// <summary>
	/// 查找第一个可用的网格位置来放置物品
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	private Array<Vector2I> FindFirstAvailbleGrids(ItemData item)
	{
		var itemShape = item.GetShape();
		for (int row = 0; row < Rows; row++)
		{
			for (int col = 0; col < Columns; col++)
			{
				if (GridItemMap.TryGetValue(new Vector2I(col, row), out var itemAtGrid) && itemAtGrid == null)
				{
					var grids = TryGetEmptyGridsByShape(new Vector2I(col, row), itemShape);
					if (grids.Count > 0)
						return grids;
				}
			}
		}
		return new Array<Vector2I>();
	}

	/// <summary>
	/// 尝试根据物品形状获取从指定位置开始的空网格
	/// </summary>
	/// <param name="start"></param>
	/// <param name="shape"></param>
	/// <returns></returns>
	private Array<Vector2I> TryGetEmptyGridsByShape(Vector2I start, Vector2I shape)
	{
		var ret = new Array<Vector2I>();
		for (int row = 0; row < shape.Y; row++)
		{
			for (int col = 0; col < shape.X; col++)
			{
				var gridId = new Vector2I(start.X + col, start.Y + row);
				if (GridItemMap.ContainsKey(gridId) && GridItemMap[gridId] == null)
				{
					ret.Add(gridId);
				}
				else
				{
					return new Array<Vector2I>();
				}
			}
		}
		return ret;
	}
}
