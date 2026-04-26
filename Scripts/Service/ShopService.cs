using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

public partial class ShopService : BaseContainerService
{
	/// <summary>
	/// 加载货物
	/// </summary>
	/// <param name="shopName"></param>
	/// <param name="goods"></param>
	public void LoadGoods(string shopName, Array<ItemData> goods)
	{
		foreach (var good in goods)
		{
			_containerRepository.GetContainer(shopName).AddItem((ItemData)good.Duplicate());
		}
	}

	/// <summary>
	/// 购买物品
	/// </summary>
	/// <param name="shopName"></param>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool Buy(string shopName, ItemData item)
	{
		return item.Buy();
	}

	/// <summary>
	/// 出售物品
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool Sell(ItemData item)
	{
		if (item.CanSell())
		{
			item.Sold();
			GBIS_CSharp.Instance.MovingItemService.ClearMovingItem();
			return true;
		}
		return false;
	}

	/// <summary>
	/// 根据商店名称获取商店
	/// </summary>
	/// <param name="containerName"></param>
	/// <returns></returns>
	public ContainerData GetContainer(string containerName)
	{
		if (GBIS_CSharp.Instance.ShopNames.Contains(containerName))
			return _containerRepository.GetContainer(containerName);
		return null;
	}
}
