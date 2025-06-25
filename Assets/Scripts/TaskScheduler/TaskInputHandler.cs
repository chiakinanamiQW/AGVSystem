using UnityEngine;
using UnityEngine.UI;
public class TaskInputHandler : MonoBehaviour
{
public InputField startNodeInput;
public InputField endNodeInput;
public InputField priorityInput;
public Text feedbackText;


public void OnSubmitTask()
{
        // 验证输入
    if (!int.TryParse(startNodeInput.text, out int startId) ||
            !int.TryParse(endNodeInput.text, out int endId) ||
            !float.TryParse(priorityInput.text, out float priority))
    {
            feedbackText.text = "输入无效，请检查格式！";
            return;
    }

        // 获取图实例
    var graph = PathfindingService.Instance._graph;

        // 检查起点节点
    var startNode = graph.GetNode(startId);
    if (startNode == null || (startNode.Type != WarehouseGraph.NodeType.Shelf && startNode.Type!=WarehouseGraph.NodeType.InPort && startNode.Type != WarehouseGraph.NodeType.OutPort))
    {
            feedbackText.text = "起点不是有效的货架节点！";
            return;
    }

        // 检查起点是否有货物
        if (startNode.Weight <= 0)
        {
            feedbackText.text = "起点货架没有货物！";
            return;
        }

        // 检查终点节点
        var endNode = graph.GetNode(endId);
        if (endNode == null || (endNode.Type != WarehouseGraph.NodeType.Shelf && endNode.Type != WarehouseGraph.NodeType.InPort && endNode.Type != WarehouseGraph.NodeType.OutPort))
        {
            feedbackText.text = "终点不是有效的货架节点！";
            return;
        }

        // 检查终点容量
        if (endNode.Weight >= endNode.WeightLimit)
        {
            feedbackText.text = "终点货架已满！";
            return;
        }

        // 创建任务
        TaskScheduler.Instance.CreateTransportTask(startId, endId, priority);
        feedbackText.text = "任务创建成功！";
    }
  
}