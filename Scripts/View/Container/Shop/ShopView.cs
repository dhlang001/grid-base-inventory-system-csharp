using Godot;
using Godot.Collections;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 商店背包视图，控制背包的绘制
/// </summary>
[Tool]
public partial class ShopView : BaseContainerView
{
	[Export] public Array<ItemData> Goods { get; set; } = new();

	/// <summary>
	/// 格子高亮
	/// </summary>
	/// <param name="gridId"></param>
	public override void GridHover(Vector2I gridId)
	{
		if (this.GetSystem<MovingItemService>().MovingItem == null)
		{
			var data = this.GetSystem<InventoryService>().FindItemDataByGrid(ContainerName, gridId);
			if (data != null)
				this.GetSystem<ItemFocusService>().FocusItem(data, ContainerName);
			return;
		}

		var movingItemView = this.GetSystem<MovingItemService>().MovingItemView;
		movingItemView.BaseSize = BaseSize;
		movingItemView.StackNumColor = StackNumColor;
		movingItemView.StackNumFont = StackNumFont;
		movingItemView.StackNumFontSize = StackNumFontSize;
		movingItemView.StackNumMargin = StackNumMargin;
	}

	/// <summary>
	/// 格子失去高亮
	/// </summary>
	/// <param name="gridId"></param>
	public override void GridLoseHover(Vector2I gridId)
	{
		this.GetSystem<ItemFocusService>().ItemLoseFocus();
	}

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			CallDeferred(MethodName.RecalculateSize);
			return;
		}

		if (string.IsNullOrEmpty(ContainerName))
		{
			GD.PushError("Shop must have a name.");
			return;
		}

		var ret = this.GetSystem<ShopService>().Regist(ContainerName, ContainerColumns, ContainerRows, true);

		if (Visible)
			this.GetModel<GBIS_Model>().OpenedContainers.Add(ContainerName);

		ContainerColumns = ret.Columns;
		ContainerRows = ret.Rows;

		this.GetSystem<ShopService>().GetContainer(ContainerName).Clear();
		this.GetSystem<ShopService>().LoadGoods(ContainerName, Goods);

		MouseFilter = MouseFilterEnum.Pass;
		InitGridContainer();
		InitItemContainer();
		InitGrids();

		this.RegisterEvent<SigInvRefreshEvent>(e => Refresh()).UnRegisterWhenNodeExitTree(this);

		VisibilityChanged += OnVisibleChanged;

		if (StackNumFont == null)
			StackNumFont = GetThemeFont("font");

		CallDeferred(MethodName.Refresh);
	}

	/// <summary>
	/// 初始化格子View
	/// </summary>
	private void InitGrids()
	{
		for (int row = 0; row < ContainerRows; row++)
		{
			for (int col = 0; col < ContainerColumns; col++)
			{
				var gridId = new Vector2I(col, row);
				var grid = new ShopGridView(this, gridId, BaseSize, GridBorderSize, GridBorderColor,
					GirdBackgroundColorEmpty, GirdBackgroundColorTaken, GirdBackgroundColorConflict, GridBackgroundColorAvilable);
				_gridContainer.AddChild(grid);
				_gridMap[gridId] = grid;
			}
		}
	}
}
