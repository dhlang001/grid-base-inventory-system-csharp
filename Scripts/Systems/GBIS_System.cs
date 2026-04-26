using Godot;
using QFramework;
using System;

namespace GridBaseInventorySystem;

public partial class GBIS_System : AbstractSystem
{

	protected override void OnInit()
	{

	}

	/// <summary>
	/// 保存背包和装备槽
	/// </summary>
	public void SaveData()
	{
		this.GetSystem<InventoryService>().SaveData();
		this.GetSystem<EquipmentSlotService>().SaveData();
	}

	/// <summary>
	/// 读取背包和装备槽
	/// </summary>
	public async void LoadData()
	{
		await this.GetModel<GBIS_Model>().GBIS_BaseNode.ToSignal(this.GetModel<GBIS_Model>().GBIS_BaseNode.GetTree(), SceneTree.SignalName.ProcessFrame);
		this.GetSystem<InventoryService>().LoadData();
		this.GetSystem<EquipmentSlotService>().LoadData();
		this.SendEvent<SigInvRefreshEvent>();
		this.SendEvent<SigSlotRefreshEvent>();
		this.SendEvent<SigShopRefreshEvent>();
	}

	/// <summary>
	/// 获取场景树的根（主要在Service中使用，因为Service没有加入场景树，所以没有 GetTree()）
	/// </summary>
	public Node GetRoot()
	{
		return this.GetModel<GBIS_Model>().GBIS_BaseNode.GetTree().Root;
	}

	/// <summary>
	/// 向背包添加物品
	/// </summary>
	public bool AddItem(string invName, ItemData itemData)
	{
		return this.GetSystem<InventoryService>().AddItem(invName, itemData);
	}

	/// <summary>
	/// 增加背包间的快速移动关系
	/// </summary>
	public void AddQuickMoveRelation(string invName, string targetInvName)
	{
		this.GetSystem<InventoryService>().AddQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 删除背包间的快速移动关系
	/// </summary>
	public void RemoveQuickMoveRelation(string invName, string targetInvName)
	{
		this.GetSystem<InventoryService>().RemoveQuickMoveRelation(invName, targetInvName);
	}

	/// <summary>
	/// 是否有正在移动的物品
	/// </summary>
	public bool HasMovingItem()
	{
		return this.GetSystem<MovingItemService>().MovingItem != null;
	}
}
