using Godot;
using QFramework;

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

		this.GetSystem<ItemFocusService>().ItemLoseFocus();

		// 处理点击动作
		if (@event.IsActionPressed(this.GetModel<GBIS_Model>().InputClick))
		{
			if (HasTaken)
			{
				if (this.GetSystem<MovingItemService>().MovingItem == null)
				{
					this.GetSystem<MovingItemService>().MoveItemByGrid(_containerView.ContainerName, GridId, Offset, _size);
				}
				else if (this.GetSystem<MovingItemService>().MovingItem is StackableData)
				{
					this.GetSystem<InventoryService>().StackMovingItem(_containerView.ContainerName, GridId);
				}
				_containerView.GridHover(GridId);
			}
			else
			{
				this.GetSystem<InventoryService>().PlaceMovingItem(_containerView.ContainerName, GridId);
			}
			return;
		}

		// 如果不是点击动作且格子没有物品，直接返回
		if (!HasTaken)
			return;

		if (@event.IsActionPressed(this.GetModel<GBIS_Model>().InputQuickMove))
		{
			this.GetSystem<InventoryService>().QuickMove(_containerView.ContainerName, GridId);
		}
		else if (@event.IsActionPressed(this.GetModel<GBIS_Model>().InputUse))
		{
			this.GetSystem<InventoryService>().UseItem(_containerView.ContainerName, GridId);
		}
		else if (@event.IsActionPressed(this.GetModel<GBIS_Model>().InputSplit) && this.GetSystem<MovingItemService>().MovingItem == null)
		{
			this.GetSystem<InventoryService>().SplitItem(_containerView.ContainerName, GridId, Offset, _size);
		}
	}
}
