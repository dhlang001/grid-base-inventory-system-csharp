using Godot;

namespace GridBaseInventorySystem;

/// <summary>
/// 丢弃物品视图，加到场景中即可，会自动全屏并移到最底层
/// </summary>
[Tool]
public partial class DropAreaView : Control
{
	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
		{
			GBIS_CSharp.Instance.MovingItemService.DropAreaView = this;
		}
		MouseFilter = Control.MouseFilterEnum.Stop;
		CallDeferred(MethodName.Resize);
		Hide();
	}

	/// <summary>
	/// 防呆，自动移到最底层，防止挡住背包导致无法放置
	/// </summary>
	public void Resize()
	{
		if (IsInsideTree())
		{
			Size = GetTree().Root.Size;
			GetParent().MoveChild(this, 0);
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event.IsActionPressed(GBIS_CSharp.Instance.InputClick))
		{
			if (GBIS_CSharp.Instance.HasMovingItem() && GBIS_CSharp.Instance.MovingItemService.MovingItem.CanDrop())
			{
				GBIS_CSharp.Instance.MovingItemService.MovingItem.Drop();
				GBIS_CSharp.Instance.MovingItemService.ClearMovingItem();
			}
		}
	}
}
