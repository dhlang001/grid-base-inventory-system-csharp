using Godot;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 物品焦点业务类
/// </summary>
public partial class ItemFocusService : AbstractSystem
{
	/// <summary>
	/// 当前焦点目标
	/// </summary>
	private ItemData CurrentItemData { get; set; }
	private string CurrentContainerName { get; set; } = "";

	protected override void OnInit()
	{

	}

	/// <summary>
	/// 焦点物品
	/// </summary>
	/// <param name="itemData"></param>
	/// <param name="containerName"></param>
	public void FocusItem(ItemData itemData, string containerName)
	{
		// 如果当前有移动物品，则不允许焦点切换
		if (this.GetSystem<MovingItemService>().MovingItem != null)
			return;

		// 如果焦点没有变化，直接返回
		if (CurrentItemData == itemData && CurrentContainerName == containerName)
			return;

		// 设置新焦点
		CurrentItemData = itemData;
		CurrentContainerName = containerName;
		this.SendEvent<SigItemFocusedEvent>(new SigItemFocusedEvent() { itemData = itemData, containerName = CurrentContainerName });
	}

	/// <summary>
	/// 焦点丢失
	/// </summary>
	public void ItemLoseFocus()
	{
		if (CurrentItemData == null)
			return;
		this.SendEvent<SigItemFocusLostEvent>(new SigItemFocusLostEvent() { itemData = (ItemData)CurrentItemData.Duplicate() });
		CurrentItemData = null;
		CurrentContainerName = "";
	}

	/// <summary>
	/// 窗口关闭时手动调用，防止关闭后提示信息一直存在
	/// </summary>
	/// <param name="containerName"></param>
	public void LostByContainerClose(string containerName)
	{
		if (CurrentContainerName != containerName)
			return;
		ItemLoseFocus();
	}
}
