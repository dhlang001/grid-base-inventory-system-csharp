using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 背包数据库，管理 ContainerData 的存取
/// </summary>
public partial class ContainerRepository : Resource
{
	private static ContainerRepository _instance;
	/// <summary>
	/// 单例
	/// </summary>
	public static ContainerRepository Instance => _instance ??= new ContainerRepository();

	/// <summary>
	/// 所有背包数据
	/// </summary>
	[Export] public Godot.Collections.Dictionary<string, ContainerData> ContainerDataMap { get; set; } = new();
	/// <summary>
	/// 所有背包的快速移动关系
	/// </summary>
	[Export] public Godot.Collections.Dictionary<string, Array<string>> QuickMoveRelationsMap { get; set; } = new();

	/// <summary>
	/// 保存所有背包数据
	/// </summary>
	public void SaveData()
	{
		ResourceSaver.Save(this, GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_ContainerData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName);
	}

	/// <summary>
	/// 读取所有背包数据
	/// </summary>
	public void LoadData()
	{
		var savedRepository = GD.Load<ContainerRepository>(
			GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSavePath + GBIS_Const.Prefix_ContainerData + GameArchitecture.Interface.GetModel<GBIS_Model>().CurrentSaveName);
		if (savedRepository == null)
			return;
		foreach (var invName in savedRepository.ContainerDataMap.Keys)
		{
			ContainerDataMap[invName] = savedRepository.ContainerDataMap[invName].DeepDuplicate();
		}
		QuickMoveRelationsMap = savedRepository.QuickMoveRelationsMap.Duplicate(true);
	}

	/// <summary>
	/// 增加并返回背包，如果已存在，返回已经注册的背包
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="columns"></param>
	/// <param name="rows"></param>
	/// <param name="avilableTypes"></param>
	/// <returns></returns>
	public ContainerData AddContainer(string invName, int columns, int rows, Array<string> avilableTypes)
	{
		var inv = GetContainer(invName);
		if (inv == null)
		{
			var newContainer = new ContainerData();
			newContainer.Init(invName, columns, rows, avilableTypes);
			ContainerDataMap[invName] = newContainer;
			return newContainer;
		}
		return inv;
	}

	/// <summary>
	/// 获取背包数据
	/// </summary>
	/// <param name="invName"></param>
	/// <returns></returns>
	public ContainerData GetContainer(string invName)
	{
		return ContainerDataMap.TryGetValue(invName, out var data) ? data : null;
	}

	/// <summary>
	/// 增加快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="targetInvName"></param>
	public void AddQuickMoveRelation(string invName, string targetInvName)
	{
		if (QuickMoveRelationsMap.ContainsKey(invName))
		{
			QuickMoveRelationsMap[invName].Add(targetInvName);
		}
		else
		{
			QuickMoveRelationsMap[invName] = new Array<string> { targetInvName };
		}
	}

	/// <summary>
	/// 移除快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="targetInvName"></param>
	public void RemoveQuickMoveRelation(string invName, string targetInvName)
	{
		if (QuickMoveRelationsMap.ContainsKey(invName))
		{
			QuickMoveRelationsMap[invName].Remove(targetInvName);
		}
	}

	/// <summary>
	/// 获取指定背包的快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <returns></returns>
	public Array<string> GetQuickMoveRelations(string invName)
	{
		return QuickMoveRelationsMap.TryGetValue(invName, out var relations) ? relations : new Array<string>();
	}
}
