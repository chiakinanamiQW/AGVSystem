using System.Collections.Generic;
using UnityEngine;
using static TaskScheduler;

public class TaskScheduler
{
    private PathfindingService _pathfindingService;

    public TaskScheduler(PathfindingService pathfindingService)
    {
        _pathfindingService = pathfindingService;
    }

    public void AssignTask(AGVAgent agv)
    {
        if (agv.CurrentTask == null) return;

        var path = _pathfindingService.PlanMultiTaskPath(agv, agv.CurrentTask);
        agv.currentPath = path;
        // 更新AGV状态，分配任务执行
    }

    public void CreateManualTask(int source, int target)
    {
        TransportTask newTask = new TransportTask
        {
            TaskID = Random.Range(0, 1000),
            Type = TaskType.Pickup,
            SourceNode = source,
            TargetNode = target,
            Priority = 1
        };

        // 假设创建新任务后立即分配给AGV执行
        AGVAgent agv = new AGVAgent();
        agv.CurrentTask = newTask;
        AssignTask(agv);
    }

    public enum TaskType
    {
        Pickup,
        Delivery,
        Charge
    }

   
}
public class TransportTask
{
    public int TaskID;
    public TaskType Type;
    public int SourceNode;
    public int TargetNode;
    public float Priority;
}
