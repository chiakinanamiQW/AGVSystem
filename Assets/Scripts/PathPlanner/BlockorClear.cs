using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockorClear : MonoBehaviour
{
    public InputField blockNodeInput;
    public InputField clearBlockNodeInput;
    public Text blockFeedbackText;
    public void OnSubmitBlocked()
    {
        if (!int.TryParse(blockNodeInput.text, out int blockId))
        {
            blockFeedbackText.text = "输入无效，请检查格式！";
            return;
        }
        var graph = PathfindingService.Instance._graph;

        // 检查起点节点
        var blockNode = graph.GetNode(blockId);
        if (blockNode == null || (blockNode.Type == WarehouseGraph.NodeType.Shelf))
        {
            blockFeedbackText.text = "堵塞点不能是货架节点！";
            return;
        }
        graph.UpdateNodeStatus(blockNode.ID, true);
    }

    public void OnSubmitClear()
    {
        if (!int.TryParse(blockNodeInput.text, out int blockId))
        {
            blockFeedbackText.text = "输入无效，请检查格式！";
            return;
        }
        var graph = PathfindingService.Instance._graph;

        // 检查起点节点
        var blockNode = graph.GetNode(blockId);
        if (blockNode == null || (blockNode.Type == WarehouseGraph.NodeType.Shelf))
        {
            blockFeedbackText.text = "清理点不能是货架节点！";
            return;
        }
        graph.UpdateNodeStatus(blockNode.ID, false);

    }
}
