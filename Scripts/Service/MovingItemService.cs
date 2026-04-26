using Godot;
using Godot.Collections;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 移动物品业务类
/// </summary>
public partial class MovingItemService : AbstractSystem
{
	/// <summary>
	/// 正在移动的物品
	/// </summary>
	public ItemData MovingItem { get; set; }
	/// <summary>
	/// 正在移动的物品View
	/// </summary>
	public ItemView MovingItemView { get; set; }
	/// <summary>
	/// 正在移动的物品的偏移（例：一个2*2的物品，点击左上角移动时，偏移是[0,0]，点击右下角移动时，偏移是[1,1]）
	/// </summary>
	public Vector2I MovingItemOffset { get; set; } = Vector2I.Zero;
	/// <summary>
	/// 丢弃物品检测区域
	/// </summary>
	public DropAreaView DropAreaView { get; set; }

	/// <summary>
	/// 顶层，用于展示移动物品的View
	/// </summary>
	private CanvasLayer _movingItemLayer { get; set; }

	protected override void OnInit()
	{

	}

	/// <summary>
	/// 获取顶层，没有则新建
	/// </summary>
	/// <returns></returns>
	public CanvasLayer GetMovingItemLayer()
	{
		if (_movingItemLayer == null)
		{
			_movingItemLayer = new CanvasLayer();
			_movingItemLayer.Layer = 128;
			this.GetSystem<GBIS_System>().GetRoot().AddChild(_movingItemLayer);
		}
		return _movingItemLayer;
	}

	/// <summary>
	/// 清除正在移动的物品
	/// </summary>
	public void ClearMovingItem()
	{
		foreach (var o in _movingItemLayer.GetChildren())
		{
			o.QueueFree();
		}
		MovingItem = null;
		MovingItemView = null;
		if (DropAreaView != null)
		{
			DropAreaView.Hide();
		}
	}

	/// <summary>
	/// 根据物品数据直接执行物品移动。
	/// 初始化移动物品的视图组件，添加到顶层图层并设置偏移，同时显示物品丢弃检测区域。
	/// </summary>
	/// <param name="itemData">待移动的物品数据实体</param>
	/// <param name="offset">物品的点击偏移坐标（格子坐标系）</param>
	/// <param name="baseSize">物品视图的基础单元格大小</param>
	public void MoveItemByData(ItemData itemData, Vector2I offset, int baseSize)
	{
		MovingItem = itemData;
		MovingItemOffset = offset;
		MovingItemView = new ItemView(itemData, baseSize);
		GetMovingItemLayer().AddChild(MovingItemView);
		MovingItemView.Move(offset);
		if (DropAreaView != null)
		{
			DropAreaView.Show();
		}
	}

	/// <summary>
	/// 根据背包格子信息获取物品并执行移动。
	/// 校验无正在移动的物品后，从指定背包格子中获取物品数据，调用移动方法并从原背包移除该物品
	/// </summary>
	/// <param name="invName">背包唯一标识名称</param>
	/// <param name="gridId">物品所在的格子坐标</param>
	/// <param name="offset">物品的点击偏移坐标（格子坐标系）</param>
	/// <param name="baseSize">物品视图的基础单元格大小</param>
	public void MoveItemByGrid(string invName, Vector2I gridId, Vector2I offset, int baseSize)
	{
		if (MovingItem != null)
		{
			GD.PushError("Already had moving item.");
			return;
		}
		var itemData = this.GetSystem<InventoryService>().FindItemDataByGrid(invName, gridId);
		if (itemData != null)
		{
			MoveItemByData(itemData, offset, baseSize);
			this.GetSystem<InventoryService>().RemoveItemByData(invName, itemData);
			if (DropAreaView != null)
			{
				DropAreaView.Show();
			}
		}
	}
}
