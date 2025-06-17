using System.Collections.Generic;
using UnityEngine;

public class PathfindingService
{
    private WarehouseGraph _graph;

    public PathfindingService(WarehouseGraph graph)
    {
        _graph = graph;
    }

    public List<int> FindPath(int start, int end, AGVAgent agv)
    {
        // A* 算法，考虑拥堵系数、故障节点和电梯跨楼层
        var openList = new List<int>();
        var closedList = new HashSet<int>();
        var gScores = new Dictionary<int, float>();
        var fScores = new Dictionary<int, float>();
        var cameFrom = new Dictionary<int, int>();

        openList.Add(start);
        gScores[start] = 0;
        fScores[start] = Heuristic(start, end);

        while (openList.Count > 0)
        {
            // 获取fScore最小的节点
            int current = GetNodeWithLowestFScore(openList, fScores);

            if (current == end)
            {
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (var edge in _graph.Edges)
            {
                if (edge.FromNode == current && !closedList.Contains(edge.ToNode))
                {
                    if (_graph.Nodes[edge.ToNode].IsBlocked) continue; // 跳过故障节点

                    float tentativeGScore = gScores[current] + edge.Distance * (1 + edge.Congestion);
                    if (!openList.Contains(edge.ToNode))
                        openList.Add(edge.ToNode);

                    if (tentativeGScore < gScores.GetValueOrDefault(edge.ToNode, float.MaxValue))
                    {
                        cameFrom[edge.ToNode] = current;
                        gScores[edge.ToNode] = tentativeGScore;
                        fScores[edge.ToNode] = gScores[edge.ToNode] + Heuristic(edge.ToNode, end);
                    }
                }
            }
        }
        return null; // 无法到达
    }

    private float Heuristic(int nodeId, int targetNodeId)
    {
        var node = _graph.Nodes[nodeId];
        var targetNode = _graph.Nodes[targetNodeId];
        return Vector3.Distance(node.Position, targetNode.Position); // 使用欧几里得距离作为启发式
    }

    private int GetNodeWithLowestFScore(List<int> openList, Dictionary<int, float> fScores)
    {
        int lowestNode = -1;
        float lowestFScore = float.MaxValue;

        foreach (var node in openList)
        {
            if (fScores.ContainsKey(node) && fScores[node] < lowestFScore)
            {
                lowestFScore = fScores[node];
                lowestNode = node;
            }
        }
        return lowestNode;
    }

    private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int currentNode)
    {
        var path = new List<int> { currentNode };
        while (cameFrom.ContainsKey(currentNode))
        {
            currentNode = cameFrom[currentNode];
            path.Insert(0, currentNode);
        }
        return path;
    }

    public List<int> PlanMultiTaskPath(AGVAgent agv, TransportTask task)
    {
        // 规划多个任务的路径：取货 -> 送货 -> 充电
        var path = FindPath(agv.CurrentNode, task.SourceNode, agv);
        path.AddRange(FindPath(task.SourceNode, task.TargetNode, agv));
        if (agv.BatteryLevel < 20) // 假设电量小于20%时需要充电 
        {
            path.AddRange(FindPath(task.TargetNode, 0, agv)); // 假设充电站在节点0
        }
        return path;
    }
}
