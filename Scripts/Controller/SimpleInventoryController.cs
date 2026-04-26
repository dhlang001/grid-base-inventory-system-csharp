using Godot;
using GridBaseInventorySystem;
using QFramework;
using System;

public partial class SimpleInventoryController : Control, IController
{

	[Export] public Godot.Collections.Array<ItemData> Items { get; set; } = new Godot.Collections.Array<ItemData>();

	[Export] public ColorRect inventory { get; set; }

	[Export] public ColorRect storage { get; set; }

	public IArchitecture GetArchitecture()
	{
		return GameArchitecture.Interface;
	}

	public override void _Ready()
	{
		base._Ready();

		this.GetSystem<GBIS_System>().AddQuickMoveRelation("demo1_inventory", "demo1_storage");
		this.GetSystem<GBIS_System>().AddQuickMoveRelation("demo1_storage", "demo1_inventory");
	}

	public void _on_button_close_inventory_pressed()
	{
		inventory.Hide();
	}
	public void _on_button_close_storage_pressed()
	{
		storage.Hide();
	}
	public void _on_button_toggle_inventory_pressed()
	{
		inventory.Visible = !inventory.Visible;
	}
	public void _on_button_toggle_storage_pressed()
	{
		storage.Visible = !storage.Visible;
	}
	public void _on_button_add_test_items_pressed()
	{
		foreach (var item in Items)
		{
			if (GD.RandRange(1, 100) > 50)
			{
				var newItem = item.Duplicate();
				((ItemData)newItem).ShaderParams = new Godot.Collections.Dictionary<string, Variant> { { "enable_enhance", true } };
				this.GetSystem<GBIS_System>().AddItem("demo1_inventory", newItem as ItemData);
			}
			else
			{
				this.GetSystem<GBIS_System>().AddItem("demo1_inventory", item);
			}
		}
	}
	public void _on_button_save_pressed()
	{
		this.GetSystem<GBIS_System>().SaveData();
	}
	public void _on_button_load_pressed()
	{
		this.GetSystem<GBIS_System>().LoadData();
	}
}
