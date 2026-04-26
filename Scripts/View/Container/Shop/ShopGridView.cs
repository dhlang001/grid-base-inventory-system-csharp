using Godot;

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
		if (@event.IsActionPressed(GBIS_CSharp.Instance.InputClick))
		{
			if (HasTaken)
			{
				if (GBIS_CSharp.Instance.MovingItemService.MovingItem == null)
				{
					var item = GBIS_CSharp.Instance.ShopService.FindItemDataByGrid(_containerView.ContainerName, GridId);
					GBIS_CSharp.Instance.ShopService.Buy(_containerView.ContainerName, item);
				}
			}
			else if (GBIS_CSharp.Instance.HasMovingItem())
			{
				GBIS_CSharp.Instance.ShopService.Sell(GBIS_CSharp.Instance.MovingItemService.MovingItem);
			}
		}
	}
}
