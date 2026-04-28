using Godot;
using Godot.Collections;
using QFramework;
using System.ComponentModel;

namespace GridBaseInventorySystem;

/// <summary>
/// 容器业务类
/// </summary>
public partial class BaseContainerService : AbstractSystem
{

	protected override void OnInit()
	{

	}

	/// <summary>
	/// 保存所有背包数据
	/// 注：可能需要重构到存档系统统一管理
	/// </summary>
	public void SaveData()
	{
		if (this.GetModel<ContainerModel>().ContainerResourceObject == null)
		{
			this.GetModel<ContainerModel>().ContainerResourceObject = new ContainerResourceObject();
		}
		ResourceSaver.Save(this.GetModel<ContainerModel>().ContainerResourceObject, GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_ContainerData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName);
	}

	/// <summary>
	/// 读取所有容器数据
	/// 注：可能需要重构到存档系统统一管理
	/// </summary>
	public void LoadData()
	{
		var saveRepository = ResourceLoader.Load<ContainerResourceObject>(GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_ContainerData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName, default, ResourceLoader.CacheMode.Ignore);
		if (saveRepository == null)
			return;
		this.GetModel<ContainerModel>().ContainerResourceObject = saveRepository.DuplicateDeep() as ContainerResourceObject;
	}

	/// <summary>
	/// 注册容器，如果重名，则返回已存在的容器
	/// </summary>
	/// <param name="containerName"></param>
	/// <param name="columns"></param>
	/// <param name="rows"></param>
	/// <param name="isShop"></param>
	/// <param name="avilableTypes"></param>
	/// <returns></returns>
	public ContainerData Regist(string containerName, int columns, int rows, bool isShop, Array<string> avilableTypes = null)
	{
		avilableTypes ??= new Array<string> { "ANY" };
		if (isShop && !this.GetModel<GBIS_Model>().ShopNames.Contains(containerName))
		{
			this.GetModel<GBIS_Model>().ShopNames.Add(containerName);
		}
		else if (!isShop && !this.GetModel<GBIS_Model>().InventoryNames.Contains(containerName))
		{
			this.GetModel<GBIS_Model>().InventoryNames.Add(containerName);
		}
		return this.GetModel<ContainerModel>().AddContainer(containerName, columns, rows, avilableTypes);
	}

	/// <summary>
	/// 通过物品名称找所有物品（同名物品可能有多个实例）
	/// </summary>
	/// <param name="containerName"></param>
	/// <param name="itemName"></param>
	/// <returns></returns>
	public Array<ItemData> FindItemDataByItemName(string containerName, string itemName)
	{
		var inv = this.GetModel<ContainerModel>().GetContainer(containerName);
		if (inv != null)
			return inv.FindItemDataByItemName(itemName);
		return new Array<ItemData>();
	}

	/// <summary>
	/// 通过格子找物品
	/// </summary>
	/// <param name="containerName"></param>
	/// <param name="gridId"></param>
	/// <returns></returns>
	public ItemData FindItemDataByGrid(string containerName, Vector2I gridId)
	{
		return this.GetModel<ContainerModel>().GetContainer(containerName).FindItemDataByGrid(gridId);
	}

	/// <summary>
	/// 尝试把物品放置到指定格子
	/// </summary>
	/// <param name="containerName"></param>
	/// <param name="itemData"></param>
	/// <param name="gridId"></param>
	/// <returns></returns>
	public bool PlaceTo(string containerName, ItemData itemData, Vector2I gridId)
	{
		if (itemData != null)
		{
			var inv = this.GetModel<ContainerModel>().GetContainer(containerName);
			if (inv != null)
			{
				var grids = inv.TryAddToGrid(itemData, gridId - this.GetSystem<MovingItemService>().MovingItemOffset);
				if (grids != null && grids.Count > 0)
				{
					this.SendEvent(new SigInvItemAddedEvent() { grids = grids, itemData = itemData, invName = containerName });
					return true;
				}
			}
		}
		return false;
	}
}
