using Godot;
using Godot.Collections;
using QFramework;
using System.ComponentModel;

namespace GridBaseInventorySystem;

/// <summary>
/// 容器业务类
/// </summary>
public partial class BaseContainerService : Node, IController
{

	public IArchitecture GetArchitecture()
	{
		return GameArchitecture.Interface;
	}

	/// <summary>
	/// 容器数据库引用
	/// </summary>
	protected ContainerRepository _containerRepository = ContainerRepository.Instance;

	/// <summary>
	/// 保存所有容器
	/// </summary>
	public void SaveData()
	{
		_containerRepository.SaveData();
	}

	/// <summary>
	/// 读取所有容器
	/// </summary>
	public void LoadData()
	{
		_containerRepository.LoadData();
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
		if (isShop && !GBIS_CSharp.Instance.ShopNames.Contains(containerName))
		{
			GBIS_CSharp.Instance.ShopNames.Add(containerName);
		}
		else if (!isShop && !GBIS_CSharp.Instance.InventoryNames.Contains(containerName))
		{
			GBIS_CSharp.Instance.InventoryNames.Add(containerName);
		}
		return _containerRepository.AddContainer(containerName, columns, rows, avilableTypes);
	}

	/// <summary>
	/// 通过物品名称找所有物品（同名物品可能有多个实例）
	/// </summary>
	/// <param name="containerName"></param>
	/// <param name="itemName"></param>
	/// <returns></returns>
	public Array<ItemData> FindItemDataByItemName(string containerName, string itemName)
	{
		var inv = _containerRepository.GetContainer(containerName);
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
		return _containerRepository.GetContainer(containerName).FindItemDataByGrid(gridId);
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
			var inv = _containerRepository.GetContainer(containerName);
			if (inv != null)
			{
				var grids = inv.TryAddToGrid(itemData, gridId - GBIS_CSharp.Instance.MovingItemService.MovingItemOffset);
				if (grids != null && grids.Count > 0)
				{
					GBIS_CSharp.Instance.EmitSignal(GBIS_CSharp.SignalName.SigInvItemAdded, containerName, itemData, grids);
					return true;
				}
			}
		}
		return false;
	}
}
