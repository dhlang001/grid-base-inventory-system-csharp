using Godot;
using GridBaseInventorySystem;
using QFramework;
using System;

public class GameArchitecture : Architecture<GameArchitecture>
{
	protected override void Init()
	{

		#region 注册 System

		this.RegisterSystem<GBIS_System>(new GBIS_System());
		this.RegisterSystem<InventoryService>(new InventoryService());
		this.RegisterSystem<ShopService>(new ShopService());
		this.RegisterSystem<EquipmentSlotService>(new EquipmentSlotService());
		this.RegisterSystem<MovingItemService>(new MovingItemService());
		this.RegisterSystem<ItemFocusService>(new ItemFocusService());

		#endregion

		#region 注册 Model

		this.RegisterModel<GBIS_Model>(new GBIS_Model());

		#endregion

		#region 注册 Utility

		#endregion
	}
}
