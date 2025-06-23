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
        // ��֤����
        if (!int.TryParse(startNodeInput.text, out int startId) ||
            !int.TryParse(endNodeInput.text, out int endId) ||
            !float.TryParse(priorityInput.text, out float priority))
        {
            feedbackText.text = "������Ч�������ʽ��";
            return;
        }

        // ��ȡͼʵ��
        var graph = PathfindingService.Instance._graph;

        // ������ڵ�
        var startNode = graph.GetNode(startId);
        if (startNode == null || startNode.Type != WarehouseGraph.NodeType.Shelf)
        {
            feedbackText.text = "��㲻����Ч�Ļ��ܽڵ㣡";
            return;
        }

        // �������Ƿ��л���
        if (startNode.Weight <= 0)
        {
            feedbackText.text = "������û�л��";
            return;
        }

        // ����յ�ڵ�
        var endNode = graph.GetNode(endId);
        if (endNode == null || endNode.Type != WarehouseGraph.NodeType.Shelf)
        {
            feedbackText.text = "�յ㲻����Ч�Ļ��ܽڵ㣡";
            return;
        }

        // ����յ�����
        if (endNode.Weight >= endNode.WeightLimit)
        {
            feedbackText.text = "�յ����������";
            return;
        }

        // ��������
        TaskScheduler.Instance.CreateTransportTask(startId, endId, priority);
        feedbackText.text = "���񴴽��ɹ���";
    }
}