using Godot;

namespace GridBaseInventorySystem;

/// <summary>
/// 格子视图，用于绘制格子
/// </summary>
public partial class InventoryGridView : BaseGridView
{
	public InventoryGridView() { }

	public InventoryGridView(BaseContainerView containerView, Vector2I gridId, int size, int borderSize, Color borderColor, Color emptyColor, Color takenColor, Color conflictColor, Color avilableColor)
		: base(containerView, gridId, size, borderSize, borderColor, emptyColor, takenColor, conflictColor, avilableColor) { }

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		if (!@event.IsPressed())
			return;

		GBIS_CSharp.Instance.ItemFocusService.ItemLoseFocus();

		// 处理点击动作
		if (@event.IsActionPressed(GBIS_CSharp.Instance.InputClick))
		{
			if (HasTaken)
			{
				if (GBIS_CSharp.Instance.MovingItemService.MovingItem == null)
				{
					GBIS_CSharp.Instance.MovingItemService.MoveItemByGrid(_containerView.ContainerName, GridId, Offset, _size);
				}
				else if (GBIS_CSharp.Instance.MovingItemService.MovingItem is StackableData)
				{
					GBIS_CSharp.Instance.InventoryService.StackMovingItem(_containerView.ContainerName, GridId);
				}
				_containerView.GridHover(GridId);
			}
			else
			{
				GBIS_CSharp.Instance.InventoryService.PlaceMovingItem(_containerView.ContainerName, GridId);
			}
			return;
		}

		// 如果不是点击动作且格子没有物品，直接返回
		if (!HasTaken)
			return;

		if (@event.IsActionPressed(GBIS_CSharp.Instance.InputQuickMove))
		{
			GBIS_CSharp.Instance.InventoryService.QuickMove(_containerView.ContainerName, GridId);
		}
		else if (@event.IsActionPressed(GBIS_CSharp.Instance.InputUse))
		{
			GBIS_CSharp.Instance.InventoryService.UseItem(_containerView.ContainerName, GridId);
		}
		else if (@event.IsActionPressed(GBIS_CSharp.Instance.InputSplit) && GBIS_CSharp.Instance.MovingItemService.MovingItem == null)
		{
			GBIS_CSharp.Instance.InventoryService.SplitItem(_containerView.ContainerName, GridId, Offset, _size);
		}
	}
}
