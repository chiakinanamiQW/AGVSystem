using System.Collections.Generic;
using UnityEngine;
using static TaskScheduler;

/// <summary>
/// 任务调度器：负责AGV（自动导引车）任务的创建、分配和管理
/// </summary>
public class TaskScheduler : MonoBehaviour
{
    private static TaskScheduler _instance;
    //public static TaskScheduler Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            _instance = FindObjectOfType<TaskScheduler>();
    //            if (_instance == null)
    //            {
    //                GameObject obj = new GameObject("TaskScheduler");
    //                _instance = obj.AddComponent<TaskScheduler>();
    //            }
    //        }
    //        return _instance;
    //    }
    //}

    public static TaskScheduler Instance
    {
        get
        {
            if (_instance == null)
            {
                // 通过 FindObjectOfType 查找现有实例
                _instance = FindObjectOfType<TaskScheduler>();

                // 如果不存在，创建一个新的 GameObject 并挂载组件
                if (_instance == null)
                {
                    GameObject obj = new GameObject("TaskScheduler");
                    _instance = obj.AddComponent<TaskScheduler>();
                    DontDestroyOnLoad(obj); // 可选：跨场景保持
                }
            }
            return _instance;
        }
    }

    private PathfindingService _pathfindingService;

    private void Awake()
    {
        _pathfindingService = PathfindingService.Instance;
        if (_pathfindingService == null)
        {
            Debug.LogError("未找到 PathfindingService 实例！");
        }
    }

    /// <summary>
    /// 创建运输任务（包含取货和送货两个子任务）
    /// </summary>
    /// <param name="source">起点货架ID</param>
    /// <param name="target">终点货架ID</param>
    /// <param name="priority">任务优先级</param>
    public void CreateTransportTask(int source, int target, float priority)
    {
        if (_pathfindingService == null)
        {
            Debug.LogError("PathfindingService 未初始化！");
            return;
        }

        // 获取图实例
        var graph = _pathfindingService._graph;

        // 创建取货任务
        TransportTask pickupTask = new TransportTask
        {
            TaskID = GenerateTaskID(),
            Type = TaskType.Pickup,
            SourceNode = CarController.Instance.currentIndex,
            TargetNode = source,
            Priority = priority,
            Status = TaskStatus.Pending,
            ShelfNodeID = source
        };

        // 创建送货任务
        TransportTask deliveryTask = new TransportTask
        {
            TaskID = GenerateTaskID(),
            Type = TaskType.Delivery,
            SourceNode = source,
            TargetNode = target,
            Priority = priority,
            Status = TaskStatus.Pending,
            ShelfNodeID = target
        };

        // 添加到任务列表并排序



        CarController.Instance.Tasks.Add(pickupTask);

        
        CarController.Instance.Tasks.Add(deliveryTask);

        // 按优先级排序，Pickup任务在前

        CarController.Instance.Tasks.Sort((a, b) =>
        {
            if (a.Priority != b.Priority)
                return b.Priority.CompareTo(a.Priority); // 高优先级在前
            return a.Type.CompareTo(b.Type); // Pickup在前
        });

        // 处理任务执行顺序
        HandleTaskExecution();
    }

    /// <summary>
    /// 为指定AGV分配任务（计算路径并更新AGV状态）
    /// </summary>
    /// <param name="agv">待分配任务的AGV实例</param>
    public void AssignTask(AGVAgent agv)
    {
        if (agv.CurrentTask == null) return;

        // 规划多阶段路径（取货->送货->可选充电）
        var path = _pathfindingService.PlanMultiTaskPath(agv, agv.CurrentTask);
        agv.CurrentPath = path;

        // 更新任务状态
        agv.CurrentTask.Status = TaskStatus.InProgress;

        // 通知小车开始移动
        CarController.Instance.CarMove(agv.CurrentTask.TargetNode);
    }

    /// <summary>
    /// 处理任务执行顺序逻辑
    /// </summary>
    private void HandleTaskExecution()
    {
        var carController = CarController.Instance;
        var agv = carController._agv;

        // 如果当前没有任务在执行
        if (agv.CurrentTask == null && carController.Tasks.Count > 0)
        {
            StartNextTask();
        }
        // 如果有更高优先级任务且小车未载货
        else if (carController.Tasks.Count > 0 &&
                 carController.Tasks[0].Priority > agv.CurrentTask?.Priority &&
                 agv.Loads <= 0)
        {
            // 中断当前任务（重新加入队列）
            var interruptedTask = agv.CurrentTask;
            interruptedTask.Status = TaskStatus.Pending;
            carController.Tasks.Add(interruptedTask);

            StartNextTask();
        }
    }

    /// <summary>
    /// 开始执行下一个任务
    /// </summary>
    private void StartNextTask()
    {
        var carController = CarController.Instance;
        if (carController.Tasks.Count == 0) return;

        var nextTask = carController.Tasks[0];
        carController.Tasks.RemoveAt(0);

        carController._agv.CurrentTask = nextTask;
        AssignTask(carController._agv);
    }

    /// <summary>
    /// 任务完成回调
    /// </summary>
    /// <param name="agv">完成任务的AGV实例</param>
    public void OnTaskCompleted(AGVAgent agv)
    {
        if (agv.CurrentTask == null) return;

        // 更新货架状态
        var graph = _pathfindingService._graph;
        var currentTask = agv.CurrentTask;

        switch (currentTask.Type)
        {
            case TaskType.Pickup:
                // 从货架取货
                var sourceNode = graph.GetNode(currentTask.SourceNode);
                float pickupAmount = Mathf.Min(sourceNode.Weight, agv.MaxLoad - agv.Loads);
                sourceNode.Weight -= pickupAmount;
                agv.Loads += pickupAmount;
                break;

            case TaskType.Delivery:
                // 送货到货架
                var targetNode = graph.GetNode(currentTask.TargetNode);
                float availableCapacity = targetNode.WeightLimit - targetNode.Weight;
                float deliveryAmount = Mathf.Min(agv.Loads, availableCapacity);
                targetNode.Weight += deliveryAmount;
                agv.Loads -= deliveryAmount;
                break;
        }

        // 更新任务状态
        currentTask.Status = TaskStatus.Completed;

        // 开始下一个任务
        StartNextTask();
    }

    /// <summary>
    /// 生成随机任务ID
    /// </summary>
    private int GenerateTaskID()
    {
        return Random.Range(1000, 9999);
    }

    /// <summary>
    /// 创建手动任务（通常由用户界面调用）
    /// </summary>
    public void CreateManualTask(int source, int target)
    {
        CreateTransportTask(source, target, 1); // 默认优先级为1
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

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        Pending,     // 等待中
        InProgress,  // 执行中
        Completed,   // 已完成
        Failed       // 失败
    }
}

/// <summary>
/// 运输任务数据类：包含AGV执行任务所需的全部信息
/// </summary>
public class TransportTask
{
    public int TaskID;          // 任务唯一标识符
    public TaskScheduler.TaskType Type;       // 任务类型（取货/送货/充电）
    public int SourceNode;      // 取货节点ID（针对取货任务）
    public int TargetNode;      // 送货节点ID（针对送货任务）
    public float Priority;      // 任务优先级（值越高越紧急）
    public TaskScheduler.TaskStatus Status;   // 任务当前状态
    public int ShelfNodeID;     // 关联的货架节点ID（用于快速访问）
}

/// <summary>
/// AGV代理类扩展
/// </summary>
public partial class AGVAgent
{
    public float MaxLoad = 100f; // AGV最大载重量
}