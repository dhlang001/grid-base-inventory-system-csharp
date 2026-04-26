using Godot;
using Godot.Collections;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 背包视图，控制背包的绘制
/// </summary>
[Tool]
public partial class InventoryView : BaseContainerView
{
	/// <summary>
	/// 允许存放的物品类型，如果背包名字重复，可存放的物品类型需要一样
	/// </summary>
	[Export] public Array<string> AvilableTypes { get; set; } = new Array<string>() { "ANY" };

	public override void GridHover(Vector2I gridId)
	{
		HandleGridHover(gridId, true);
	}

	public override void GridLoseHover(Vector2I gridId)
	{
		HandleGridHover(gridId, false);
	}

	private void HandleGridHover(Vector2I gridId, bool isHover)
	{
		if (this.GetSystem<MovingItemService>().MovingItem == null)
		{
			var data = this.GetSystem<InventoryService>().FindItemDataByGrid(this.ContainerName, gridId);
			if (data != null)
			{
				if (isHover)
					this.GetSystem<ItemFocusService>().FocusItem(data, this.ContainerName);
				else
					this.GetSystem<ItemFocusService>().ItemLoseFocus();
			}
			return;
		}

		// 下面是对正在移动的物体的处理
		if (isHover)
		{
			var movingItemView = this.GetSystem<MovingItemService>().MovingItemView;
			movingItemView.BaseSize = BaseSize;
			movingItemView.StackNumColor = StackNumColor;
			movingItemView.StackNumFont = StackNumFont;
			movingItemView.StackNumFontSize = StackNumFontSize;
			movingItemView.StackNumMargin = StackNumMargin;
		}

		var movingItemOffset = this.GetSystem<MovingItemService>().MovingItemOffset;
		var movingItem = this.GetSystem<MovingItemService>().MovingItem;
		var itemShape = movingItem.GetShape();
		var grids = GetGridsByShape(gridId - movingItemOffset, itemShape);

		bool hasConflict = false;
		if (isHover)
		{
			hasConflict = itemShape.X * itemShape.Y != grids.Count || !this.GetSystem<InventoryService>().GetContainer(ContainerName).IsItemAvilable(movingItem);
			foreach (var grid in grids)
			{
				if (hasConflict)
					break;
				hasConflict = this._gridMap[grid].HasTaken;
				var itemData = this.GetSystem<InventoryService>().FindItemDataByGrid(this.ContainerName, gridId);
				if (hasConflict && itemData != null)
				{
					if (itemData is StackableData stackable)
					{
						if (itemData.ItemName == this.GetSystem<MovingItemService>().MovingItem.ItemName && !stackable.IsFull())
						{
							hasConflict = false;
						}
					}
				}
			}
		}

		foreach (var grid in grids)
		{
			var gridView = this._gridMap[grid];
			if (isHover)
			{
				gridView.State = hasConflict ? GridState.Conflict : GridState.Avilable;
			}
			else
			{
				gridView.State = gridView.HasTaken ? GridState.Taken : GridState.Empty;
			}
		}
	}

	public void ChangeDataSource(string newContainerName)
	{
		foreach (var child in GetChildren())
		{
			child.QueueFree();
		}
		var ret = this.GetSystem<InventoryService>().Regist(newContainerName, this.ContainerColumns, this.ContainerRows, false, this.AvilableTypes);
		this.ContainerName = newContainerName;
		this.AvilableTypes = ret.AvilableTypes;
		this.ContainerColumns = ret.Columns;
		this.ContainerRows = ret.Rows;
		this.InitGridContainer();
		this.InitItemContainer();
		this.InitGrids();
		this.CallDeferred(MethodName.Refresh);
	}

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			this.CallDeferred(MethodName.RecalculateSize);
			return;
		}

		if (string.IsNullOrEmpty(this.ContainerName))
		{
			GD.PushError("Inventory must have a name.");
			return;
		}

		var ret = this.GetSystem<InventoryService>().Regist(this.ContainerName, this.ContainerColumns, this.ContainerRows, false, this.AvilableTypes);

		if (this.Visible)
			this.GetModel<GBIS_Model>().OpenedContainers.Add(this.ContainerName);

		if (!this.GetModel<GBIS_Model>().InventoryNames.Contains(this.ContainerName))
			this.GetModel<GBIS_Model>().InventoryNames.Add(this.ContainerName);

		// 使用已注册的信息覆盖View设置
		this.AvilableTypes = ret.AvilableTypes;
		this.ContainerColumns = ret.Columns;
		this.ContainerRows = ret.Rows;

		this.MouseFilter = MouseFilterEnum.Pass;
		this.InitGridContainer();
		this.InitItemContainer();
		this.InitGrids();
		this.RegisterEvent<SigInvItemAddedEvent>(e => OnItemAdded(e.invName, e.itemData, e.grids)).UnRegisterWhenNodeExitTree(this);
		this.RegisterEvent<SigInvItemRemovedEvent>(e => OnItemRemoved(e.invName, e.itemData)).UnRegisterWhenNodeExitTree(this);
		this.RegisterEvent<SigInvItemUpdatedEvent>(e => OnInvItemUpdated(e.invName, e.gridId)).UnRegisterWhenNodeExitTree(this);
		this.RegisterEvent<SigInvRefreshEvent>(e => Refresh()).UnRegisterWhenNodeExitTree(this);

		this.VisibilityChanged += OnVisibleChanged;

		if (this.StackNumFont == null)
			this.StackNumFont = GetThemeFont("font");

		CallDeferred(MethodName.Refresh);
	}

	/// <summary>
	/// 监听添加物品
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="itemData"></param>
	/// <param name="grids"></param>
	private void OnItemAdded(string invName, ItemData itemData, Array<Vector2I> grids)
	{
		if (invName != this.ContainerName)
			return;
		if (!IsVisibleInTree())
			return;

		var item = DrawItem(itemData, grids[0]);
		this._items.Add(item);
		this._itemGridsMap[item] = grids;
		foreach (var grid in grids)
		{
			this._gridMap[grid].Taken(grid - grids[0]);
			this._gridItemMap[grid] = item;
		}
	}

	/// <summary>
	/// 监听移除物品
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="itemData"></param>
	private void OnItemRemoved(string invName, ItemData itemData)
	{
		if (invName != this.ContainerName)
			return;
		if (!this.IsVisibleInTree())
			return;

		for (int i = this._items.Count - 1; i >= 0; i--)
		{
			var item = this._items[i];
			if (item.Data == itemData)
			{
				var grids = this._itemGridsMap[item];
				foreach (var grid in grids)
				{
					this._gridMap[grid].Release();
					this._gridItemMap[grid] = null;
				}
				item.QueueFree();
				this._items.RemoveAt(i);
				break;
			}
		}
	}

	/// <summary>
	/// 监听更新物品
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="gridId"></param>
	private void OnInvItemUpdated(string invName, Vector2I gridId)
	{
		if (invName != this.ContainerName)
			return;
		if (!this.IsVisibleInTree())
			return;

		this._gridItemMap[gridId]?.QueueRedraw();
	}

	/// <summary>
	/// 绘制物品
	/// </summary>
	/// <param name="itemData"></param>
	/// <param name="firstGrid"></param>
	/// <returns></returns>
	protected override ItemView DrawItem(ItemData itemData, Vector2I firstGrid)
	{
		var item = new ItemView(itemData, this.BaseSize, this.StackNumFont, this.StackNumFontSize, this.StackNumMargin, this.StackNumColor);
		this._itemContainer.AddChild(item);
		item.GlobalPosition = this._gridMap[firstGrid].GlobalPosition;
		return item;
	}

	/// <summary>
	/// 初始化格子View
	/// </summary>
	private void InitGrids()
	{
		this._gridMap.Clear();
		for (int row = 0; row < this.ContainerRows; row++)
		{
			for (int col = 0; col < this.ContainerColumns; col++)
			{
				var gridId = new Vector2I(col, row);
				var grid = new InventoryGridView(this, gridId, this.BaseSize, this.GridBorderSize, this.GridBorderColor, this.GirdBackgroundColorEmpty, this.GirdBackgroundColorTaken, this.GirdBackgroundColorConflict, this.GridBackgroundColorAvilable);
				this._gridContainer.AddChild(grid);
				this._gridMap[gridId] = grid;
			}
		}
	}
}
