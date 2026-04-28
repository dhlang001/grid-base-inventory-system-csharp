using Godot;
using Godot.Collections;
using QFramework;
using System;

namespace GridBaseInventorySystem;

public partial class ContainerModel : AbstractModel
{
	/// <summary>
	/// ContainerResourceObject 实例
	/// </summary>
	public ContainerResourceObject ContainerResourceObject { get; set; }

	protected override void OnInit()
	{
		ContainerResourceObject ??= new ContainerResourceObject();
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
			ContainerResourceObject.ContainerDataMap[invName] = newContainer;
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
		return ContainerResourceObject.ContainerDataMap.TryGetValue(invName, out var data) ? data : null;
	}

	/// <summary>
	/// 增加快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="targetInvName"></param>
	public void AddQuickMoveRelation(string invName, string targetInvName)
	{
		if (ContainerResourceObject.QuickMoveRelationsMap.ContainsKey(invName))
		{
			ContainerResourceObject.QuickMoveRelationsMap[invName].Add(targetInvName);
		}
		else
		{
			ContainerResourceObject.QuickMoveRelationsMap[invName] = new Array<string> { targetInvName };
		}
	}

	/// <summary>
	/// 移除快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <param name="targetInvName"></param>
	public void RemoveQuickMoveRelation(string invName, string targetInvName)
	{
		if (ContainerResourceObject.QuickMoveRelationsMap.ContainsKey(invName))
		{
			ContainerResourceObject.QuickMoveRelationsMap[invName].Remove(targetInvName);
		}
	}

	/// <summary>
	/// 获取指定背包的快速移动关系
	/// </summary>
	/// <param name="invName"></param>
	/// <returns></returns>
	public Array<string> GetQuickMoveRelations(string invName)
	{
		return ContainerResourceObject.QuickMoveRelationsMap.TryGetValue(invName, out var relations) ? relations : new Array<string>();
	}
}
