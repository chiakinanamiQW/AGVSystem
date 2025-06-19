using System.Collections.Generic;
using Unity.Collections;
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
        Debug.Log(_graph.Nodes.Find(node => node.ID == start).Position);
        Debug.Log(_graph.Nodes.Find(node => node.ID == end).Position);

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
                    if (_graph.Nodes.Find(node => node.ID == edge.ToNode).IsBlocked) continue; // 跳过故障节点

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
        var node = _graph.Nodes.Find(node => node.ID == nodeId);
        var targetNode = _graph.Nodes.Find(node1 => node1.ID == targetNodeId);
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

    public void GenerateTestGraph()
    {
        // 清除现有图数据
        _graph = new WarehouseGraph();

    // 创建4层仓库
        for (int floor = 0; floor < 4; floor++)
        {
            // 添加路径点 (3x5网格布局)
            int[,] pathPointIds = new int[3,5];
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    int id = floor * 1000 + row * 10 + col;
                    Vector3 pos = new Vector3(col * 20, floor * 10, row * 15);
                    _graph.AddNode(id, WarehouseGraph.NodeType.PathPoint, pos, floor, float.MaxValue);
                    pathPointIds[row, col] = id;
                }
            }
            
            // 连接路径点 (网格结构)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    // 水平连接
                    if (col < 4)
                    {
                        float dist = Vector3.Distance(
                            _graph.Nodes.Find(n => n.ID == pathPointIds[row, col]).Position,
                            _graph.Nodes.Find(n => n.ID == pathPointIds[row, col + 1]).Position
                        );
                        _graph.addDoubleEdge(pathPointIds[row, col], pathPointIds[row, col + 1], dist, true, 0.1f);
                    }
                    
                    // 垂直连接
                    if (row < 2)
                    {
                        float dist = Vector3.Distance(
                            _graph.Nodes.Find(n => n.ID == pathPointIds[row, col]).Position,
                            _graph.Nodes.Find(n => n.ID == pathPointIds[row + 1, col]).Position
                        );
                        _graph.addDoubleEdge(pathPointIds[row, col], pathPointIds[row + 1, col], dist, true, 0.1f);
                    }
                }
            }

            // 添加货架 (每层20个)
            for (int i = 0; i < 20; i++)
            {
                int id = floor * 1000 + 100 + i;
                int row = i / 10;  // 0-1行
                int col = i % 5;   // 0-4列
                
                // 左右两列货架
                float xPos = (i % 10 < 5) ? -5f : 105f;
                Vector3 pos = new Vector3(xPos, floor * 10, row * 15 + 7.5f);
                
                _graph.AddNode(id, WarehouseGraph.NodeType.Shelf, pos, floor, 1000f);  // 货架承重1000kg
                
                // 连接到最近的路径点
                int nearestPathId = pathPointIds[row, (i % 5)];
                float dist = Vector3.Distance(pos, _graph.Nodes.Find(n => n.ID == nearestPathId).Position);
                _graph.addDoubleEdge(id, nearestPathId, dist, true, 0.2f);
            }

            // 添加电梯 (每层2个)
            for (int i = 0; i < 2; i++)
            {
                int id = floor * 1000 + 200 + i;
                Vector3 pos = new Vector3(i * 100, floor * 10, 22.5f);  // 电梯位于两端
                _graph.AddNode(id, WarehouseGraph.NodeType.Elevator, pos, floor, float.MaxValue);
                
                // 连接到中央路径点
                int connectPoint = pathPointIds[1, i == 0 ? 0 : 4];
                float dist = Vector3.Distance(pos, _graph.Nodes.Find(n => n.ID == connectPoint).Position);
                _graph.addDoubleEdge(id, connectPoint, dist, true, 0.3f);
            }

            // 1-2层添加充电点
            if (floor < 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    int id = floor * 1000 + 300 + i;
                    Vector3 pos = new Vector3(40 + i * 20, floor * 10, 30f);
                    _graph.AddNode(id, WarehouseGraph.NodeType.Charger, pos, floor, float.MaxValue);
                    
                    // 连接到最近的路径点
                    int nearestPathId = pathPointIds[2, 2];
                    float dist = Vector3.Distance(pos, _graph.Nodes.Find(n => n.ID == nearestPathId).Position);
                    _graph.addDoubleEdge(id, nearestPathId, dist, true, 0.1f);
                }
            }

            // 1层添加入库口和出库口
            if (floor == 0)
            {
                // 入库口
                int inPortId = 4000;
                Vector3 inPos = new Vector3(50, 0, -5);
                _graph.AddNode(inPortId, WarehouseGraph.NodeType.InPort, inPos, 0, float.MaxValue);
                
                // 连接到最近的路径点
                int nearestInPath = pathPointIds[0, 2];
                float inDist = Vector3.Distance(inPos, _graph.Nodes.Find(n => n.ID == nearestInPath).Position);
                _graph.addDoubleEdge(inPortId, nearestInPath, inDist, true, 0.1f);
                
                // 出库口
                int outPortId = 4001;
                Vector3 outPos = new Vector3(50, 0, 40);
                _graph.AddNode(outPortId, WarehouseGraph.NodeType.OutPort, outPos, 0, float.MaxValue);
                
                // 连接到最近的路径点
                int nearestOutPath = pathPointIds[2, 2];
                float outDist = Vector3.Distance(outPos, _graph.Nodes.Find(n => n.ID == nearestOutPath).Position);
                _graph.addDoubleEdge(outPortId, nearestOutPath, outDist, true, 0.1f);
            }
        }
    }

    public void GenerateTestGraph1()
    {
        int id = 0;
        for(int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                _graph.AddNode(id, WarehouseGraph.NodeType.PathPoint, Vector3.zero + new Vector3(j, i, 0), 0,
                    float.MaxValue);
                id++;
            }
        }

        for (int i = 0; i < 20; i++)
        {
            _graph.AddEdge(i, i + 5, 10.0f, true, 0.1f);
            _graph.AddEdge(i + 5, i, 10.0f, true, 0.1f);

        }
        
        for(int i = 0; i < 4; i++)
        {
            for (int j = i; j < i + 21; j += 5)
            {
                _graph.AddEdge(j, j + 1, 10.0f, true, 0.1f);
                _graph.AddEdge(j + 1, j, 10.0f, true, 0.1f);

            }
        }
    }
}
