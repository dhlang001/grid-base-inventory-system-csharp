using Godot;
using Godot.Collections;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 背包视图，控制背包的绘制
/// </summary>
public partial class BaseContainerView : Control, IController
{

	[ExportGroup("Container Settings")]
	/// <summary>
	/// 背包名字，如果重复，则显示同一来源的数据
	/// </summary>
	[Export] public string ContainerName { get; set; } = GBIS_Const.DefaultInventoryName;

	private int _containerColumns = 2;
	/// <summary>
	/// 背包列数，如果背包名字重复，列数需要一样
	/// </summary>
	[Export]
	public int ContainerColumns
	{
		get => _containerColumns;
		set { _containerColumns = value; RecalculateSize(); }
	}

	private int _containerRows = 2;
	/// <summary>
	/// 背包行数，如果背包名字重复，行数需要一样
	/// </summary>
	[Export]
	public int ContainerRows
	{
		get => _containerRows;
		set { _containerRows = value; RecalculateSize(); }
	}

	[ExportGroup("Grid Settings")]
	private int _baseSize = 32;
	/// <summary>
	/// 格子大小
	/// </summary>
	[Export]
	public int BaseSize
	{
		get => _baseSize;
		set { _baseSize = value; RecalculateSize(); }
	}

	private int _gridBorderSize = 1;
	/// <summary>
	/// 格子边框大小
	/// </summary>
	[Export]
	public int GridBorderSize
	{
		get => _gridBorderSize;
		set { _gridBorderSize = value; QueueRedraw(); }
	}

	private Color _gridBorderColor = BaseGridView.DefaultBorderColor;
	/// <summary>
	/// 格子边框颜色
	/// </summary>
	[Export]
	public Color GridBorderColor
	{
		get => _gridBorderColor;
		set { _gridBorderColor = value; QueueRedraw(); }
	}

	private Color _girdBackgroundColorEmpty = BaseGridView.DefaultEmptyColor;
	/// <summary>
	/// 格子空置颜色
	/// </summary>
	[Export]
	public Color GirdBackgroundColorEmpty
	{
		get => _girdBackgroundColorEmpty;
		set { _girdBackgroundColorEmpty = value; QueueRedraw(); }
	}

	private Color _girdBackgroundColorTaken = BaseGridView.DefaultTakenColor;
	/// <summary>
	/// 格子占用颜色
	/// </summary>
	[Export]
	public Color GirdBackgroundColorTaken
	{
		get => _girdBackgroundColorTaken;
		set { _girdBackgroundColorTaken = value; QueueRedraw(); }
	}

	private Color _girdBackgroundColorConflict = BaseGridView.DefaultConflictColor;
	/// <summary>
	/// 格子冲突颜色
	/// </summary>
	[Export]
	public Color GirdBackgroundColorConflict
	{
		get => _girdBackgroundColorConflict;
		set { _girdBackgroundColorConflict = value; QueueRedraw(); }
	}

	private Color _gridBackgroundColorAvilable = BaseGridView.DefaultAvilableColor;
	/// <summary>
	/// 格子可用颜色
	/// </summary>
	[Export]
	public Color GridBackgroundColorAvilable
	{
		get => _gridBackgroundColorAvilable;
		set { _gridBackgroundColorAvilable = value; QueueRedraw(); }
	}

	[ExportGroup("Stack Settings")]
	private Font _stackNumFont;
	/// <summary>
	/// 堆叠数量的字体
	/// </summary>
	[Export]
	public Font StackNumFont
	{
		get => _stackNumFont;
		set { _stackNumFont = value; QueueRedraw(); }
	}

	private int _stackNumFontSize = 16;
	/// <summary>
	/// 堆叠数量的字体大小
	/// </summary>
	[Export]
	public int StackNumFontSize
	{
		get => _stackNumFontSize;
		set { _stackNumFontSize = value; QueueRedraw(); }
	}

	private int _stackNumMargin = 4;
	/// <summary>
	/// 堆叠数量的边距（右下角）
	/// </summary>
	[Export]
	public int StackNumMargin
	{
		get => _stackNumMargin;
		set { _stackNumMargin = value; QueueRedraw(); }
	}

	private Color _stackNumColor = Colors.White;
	/// <summary>
	/// 堆叠数量的颜色
	/// </summary>
	[Export]
	public Color StackNumColor
	{
		get => _stackNumColor;
		set { _stackNumColor = value; QueueRedraw(); }
	}

	/// <summary>
	/// 格子容器
	/// </summary>
	protected GridContainer _gridContainer;
	/// <summary>
	/// 物品容器
	/// </summary>
	protected Control _itemContainer;

	/// <summary>
	/// 所有物品的View
	/// </summary>
	protected Array<ItemView> _items = new();
	/// <summary>
	/// 物品到格子的映射（Array[Vector2i]）
	/// </summary>
	protected Godot.Collections.Dictionary<ItemView, Array<Vector2I>> _itemGridsMap = new();
	/// <summary>
	/// 格子到格子View的映射
	/// </summary>
	protected Godot.Collections.Dictionary<Vector2I, BaseGridView> _gridMap = new();
	/// <summary>
	/// 格子到物品的映射
	/// </summary>
	protected Godot.Collections.Dictionary<Vector2I, ItemView> _gridItemMap = new();

	public IArchitecture GetArchitecture()
	{
		return GameArchitecture.Interface;
	}

	/// <summary>
	/// 刷新背包显示
	/// </summary>
	public virtual void Refresh()
	{
		ClearInv();
		ContainerData containerData = this.GetSystem<InventoryService>().GetContainer(ContainerName);
		if (containerData == null)
			containerData = this.GetSystem<ShopService>().GetContainer(ContainerName);

		var handledItem = new Godot.Collections.Dictionary<ItemData, ItemView>();
		foreach (var grid in _gridMap.Keys)
		{
			ItemData itemData = null;
			containerData.GridItemMap.TryGetValue(grid, out itemData);
			if (itemData != null && !handledItem.ContainsKey(itemData))
			{
				var grids = containerData.ItemGridsMap[itemData];
				var item = DrawItem(itemData, grids[0]);
				handledItem[itemData] = item;
				_items.Add(item);
				_itemGridsMap[item] = grids;
				foreach (var g in grids)
				{
					_gridMap[g].Taken(g - grids[0]);
					_gridItemMap[g] = item;
				}
				continue;
			}
			else if (itemData != null)
			{
				_gridItemMap[grid] = handledItem[itemData];
			}
			else
			{
				_gridItemMap[grid] = null;
			}
		}
	}

	/// <summary>
	/// 通过格子ID获取物品视图
	/// </summary>
	/// <param name="gridId"></param>
	/// <returns></returns>
	public ItemView FindItemViewByGrid(Vector2I gridId)
	{
		_gridItemMap.TryGetValue(gridId, out var itemView);
		return itemView;
	}

	public virtual void GridHover(Vector2I gridId) { }
	public virtual void GridLoseHover(Vector2I gridId) { }

	protected async void OnVisibleChanged()
	{
		if (IsVisibleInTree())
		{
			this.GetModel<GBIS_Model>().OpenedContainers.Add(ContainerName);
			// 需要等待GirdContainer处理完成，否则其下的所有grid没有position信息
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			CallDeferred(MethodName.Refresh);
		}
		else
		{
			this.GetModel<GBIS_Model>().OpenedContainers.Remove(ContainerName);
		}
	}

	/// <summary>
	/// 清空背包显示
	/// 注意，只清空显示，不清空数据库
	/// </summary>
	protected void ClearInv()
	{
		foreach (var item in _items)
		{
			item.QueueFree();
		}
		_items.Clear();
		_itemGridsMap.Clear();
		foreach (var grid in _gridMap.Values)
		{
			grid.Release();
		}
		_gridItemMap.Clear();
	}

	/// <summary>
	/// 从指定格子开始，获取形状覆盖的格子
	/// </summary>
	/// <param name="start"></param>
	/// <param name="shape"></param>
	/// <returns></returns>
	protected Array<Vector2I> GetGridsByShape(Vector2I start, Vector2I shape)
	{
		var ret = new Array<Vector2I>();
		for (int row = 0; row < shape.Y; row++)
		{
			for (int col = 0; col < shape.X; col++)
			{
				var gridId = new Vector2I(start.X + col, start.Y + row);
				if (_gridMap.ContainsKey(gridId))
					ret.Add(gridId);
			}
		}
		return ret;
	}

	/// <summary>
	/// 绘制物品
	/// </summary>
	/// <param name="itemData"></param>
	/// <param name="firstGrid"></param>
	/// <returns></returns>
	protected virtual ItemView DrawItem(ItemData itemData, Vector2I firstGrid)
	{
		var item = new ItemView(itemData, _baseSize, _stackNumFont, _stackNumFontSize, _stackNumMargin, _stackNumColor);
		_itemContainer.AddChild(item);
		item.GlobalPosition = _gridMap[firstGrid].GlobalPosition;
		return item;
	}

	/// <summary>
	/// 初始化格子容器
	/// </summary>
	protected void InitGridContainer()
	{
		_gridContainer = new GridContainer();
		_gridContainer.AddThemeConstantOverride("h_separation", 0);
		_gridContainer.AddThemeConstantOverride("v_separation", 0);
		_gridContainer.Columns = _containerColumns;
		_gridContainer.MouseFilter = MouseFilterEnum.Ignore;
		AddChild(_gridContainer);
	}

	/// <summary>
	/// 初始化物品容器
	/// </summary>
	protected void InitItemContainer()
	{
		_itemContainer = new Control();
		AddChild(_itemContainer);
	}

	/// <summary>
	/// 编辑器中绘制示例
	/// </summary>
	public override void _Draw()
	{
		if (Engine.IsEditorHint())
		{
			int innerSize = _baseSize - _gridBorderSize * 2;
			for (int row = 0; row < _containerRows; row++)
			{
				for (int col = 0; col < _containerColumns; col++)
				{
					DrawRect(new Rect2(col * _baseSize, row * _baseSize, _baseSize, _baseSize), _gridBorderColor);
					DrawRect(new Rect2(col * _baseSize + _gridBorderSize, row * _baseSize + _gridBorderSize, innerSize, innerSize), _girdBackgroundColorEmpty);
					var font = _stackNumFont ?? GetThemeFont("font");
					var textSize = font.GetStringSize("99", HorizontalAlignment.Right, -1, _stackNumFontSize);
					var pos = new Vector2(
						_baseSize - textSize.X - _stackNumMargin,
						_baseSize - font.GetDescent(_stackNumFontSize) - _stackNumMargin
					);
					pos += new Vector2(col * _baseSize, row * _baseSize);
					DrawString(font, pos, "99", HorizontalAlignment.Right, -1, _stackNumFontSize, _stackNumColor);
				}
			}
		}
	}

	/// <summary>
	/// 重新计算大小
	/// </summary>
	protected void RecalculateSize()
	{
		var newSize = new Vector2(_containerColumns * _baseSize, _containerRows * _baseSize);
		if (Size != newSize)
			Size = newSize;
		QueueRedraw();
	}
}
