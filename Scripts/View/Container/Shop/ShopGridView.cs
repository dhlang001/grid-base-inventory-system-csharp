using Godot;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 商店格子视图，用于绘制格子
/// </summary>
public partial class ShopGridView : BaseGridView
{
	public ShopGridView() { }

	public ShopGridView(BaseContainerView containerView, Vector2I gridId, int size, int borderSize, Color borderColor, Color emptyColor, Color takenColor, Color conflictColor, Color avilableColor)
		: base(containerView, gridId, size, borderSize, borderColor, emptyColor, takenColor, conflictColor, avilableColor) { }

	public override void _GuiInput(InputEvent @event)
	{
		if (@event.IsActionPressed(this.GetModel<GBIS_Model>().InputClick))
		{
			if (HasTaken)
			{
				if (this.GetSystem<MovingItemService>().MovingItem == null)
				{
					var item = this.GetSystem<ShopService>().FindItemDataByGrid(_containerView.ContainerName, GridId);
					this.GetSystem<ShopService>().Buy(_containerView.ContainerName, item);
				}
			}
			else if (this.GetSystem<GBIS_System>().HasMovingItem())
			{
				this.GetSystem<ShopService>().Sell(this.GetSystem<MovingItemService>().MovingItem);
			}
		}
	}
}
