using Godot;
using GridBaseInventorySystem;
using QFramework;
using System;

public partial class ItemInfoController : ColorRect, IController
{

	[Export] public Label item_name_label { get; set; }

	public IArchitecture GetArchitecture()
	{
		return GameArchitecture.Interface;
	}

	public override void _Ready()
	{
		base._Ready();
		this.Hide();
		this.RegisterEvent<SigItemFocusedEvent>(SigItemFocused).UnRegisterWhenNodeExitTree(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		Position = this.GetGlobalMousePosition() + (Vector2.One * 5);
	}

	private void SigItemFocused(SigItemFocusedEvent e)
	{
		this.Show();
		if (this.GetModel<GBIS_Model>().ShopNames.IndexOf(e.containerName) != -1)
		{
			item_name_label.Text = $"[Shop] {e.itemData.ItemName}";
		}
		else
		{
			item_name_label.Text = e.itemData.ItemName;
		}
		this.RegisterEvent<SigItemFocusLostEvent>(SigItemFocusLost).UnRegisterWhenNodeExitTree(this);
	}

	private void SigItemFocusLost(SigItemFocusLostEvent e)
	{
		this.Hide();
	}
}
