using System.Collections.Generic;
using UnityEngine;
using static TaskScheduler;

/// <summary>
/// 任务调度器：负责AGV（自动导引车）任务的创建、分配和管理
/// </summary>
public class TaskScheduler
{
    private PathfindingService _pathfindingService; // 路径规划服务，用于计算节点间路线

    /// <summary>
    /// 构造函数：初始化任务调度器
    /// </summary>
    /// <param name="pathfindingService">路径规划服务实例</param>
    public TaskScheduler(PathfindingService pathfindingService)
    {
        _pathfindingService = pathfindingService;
    }

    /// <summary>
    /// 为指定AGV分配任务（计算路径并更新AGV状态）
    /// </summary>
    /// <param name="agv">待分配任务的AGV实例</param>
    public void AssignTask(AGVAgent agv)
    {
        if (agv.CurrentTask == null) return; // 无待执行任务时跳过

        // 规划多阶段路径（取货->送货->可选充电）
        var path = _pathfindingService.PlanMultiTaskPath(agv, agv.CurrentTask);
        agv.currentPath = path; // 更新AGV路径
        // 注：完整实现中此处还应更新AGV工作状态
    }

    /// <summary>
    /// 创建手动任务（通常由用户界面调用）
    /// </summary>
    /// <param name="source">取货节点ID</param>
    /// <param name="target">送货节点ID</param>
    public void CreateManualTask(int source, int target)
    {
        // 创建新运输任务
        TransportTask newTask = new TransportTask
        {
            TaskID = Random.Range(0, 1000), // 生成随机任务ID
            Type = TaskType.Pickup,        // 默认取货任务
            SourceNode = source,          // 设置取货位置
            TargetNode = target,          // 设置送货位置
            Priority = 1                   // 默认优先级（1=普通）
        };

        // 实际实现中应分配给空闲AGV
        // 此处创建临时AGV用于演示
        AGVAgent agv = new AGVAgent();
        agv.CurrentTask = newTask;
        AssignTask(agv); // 立即分配任务
    }

    /// <summary>
    /// 任务类型枚举
    /// </summary>
    public enum TaskType
    {
        Pickup,    // 取货任务
        Delivery,  // 送货任务
        Charge     // 充电任务
    }
}

/// <summary>
/// 运输任务数据类：包含AGV执行任务所需的全部信息
/// </summary>
public class TransportTask
{
    public int TaskID;          // 任务唯一标识符
    public TaskType Type;       // 任务类型（取货/送货/充电）
    public int SourceNode;      // 取货节点ID（针对取货任务）
    public int TargetNode;      // 送货节点ID（针对送货任务）
    public float Priority;      // 任务优先级（值越高越紧急）
}

//做一个输入端，任务列表，普通任务排序