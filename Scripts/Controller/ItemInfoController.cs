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
		GBIS_CSharp.Instance.SigItemFocused += SigItemFocused;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		Position = this.GetGlobalMousePosition() + (Vector2.One * 5);
	}

	private void SigItemFocused(ItemData item_data, string containerName)
	{
		this.Show();
		if (GBIS_CSharp.Instance.ShopNames.IndexOf(containerName) != -1)
		{
			item_name_label.Text = $"[Shop] {item_data.ItemName}";
		}
		else
		{
			item_name_label.Text = item_data.ItemName;
		}
		GBIS_CSharp.Instance.SigItemFocusLost += SigItemFocusLost;
	}

	private void SigItemFocusLost(ItemData itemData)
	{
		this.Hide();
	}
}
