using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class PathfindingService
{
    private static PathfindingService _instance = null;
    
    public static PathfindingService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PathfindingService();
            }
            
            return _instance;
        }
    }
    public WarehouseGraph _graph;

    private PathfindingService(WarehouseGraph graph)
    {
        _graph = graph;
    }

    protected PathfindingService()
    {
        _graph = new WarehouseGraph();
    }

    public List<int> FindPath(int start, int end, AGVAgent agv)//寻找start到end的路径
    {
        if (_graph == null || _graph.GetNode(start) == null || _graph.GetNode(end) == null)
            return null;
        
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
    
    public void DrawGraphGizmos()//绘制仓库图gizmos
    {
        foreach (var edge in _graph.Edges)
        {
            var fromNode = _graph.GetNode(edge.FromNode);
            var toNode = _graph.GetNode(edge.ToNode);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(fromNode.Position, toNode.Position);
        }

        foreach (var node in _graph.Nodes)
        {
            switch (node.Type)
            {
                case WarehouseGraph.NodeType.PathPoint:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(node.Position, 0.1f);
                    break;
                case WarehouseGraph.NodeType.Shelf:
                    Gizmos.color = Color.gray;
                    Gizmos.DrawCube(node.Position, new Vector3(3f, 2.0f, 3f));
                    break;
                case WarehouseGraph.NodeType.Elevator:
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(node.Position, new Vector3(1.1f, 5f, 1.0f));
                    break;
                case WarehouseGraph.NodeType.Charger:
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(node.Position, new Vector3(3f, 1.0f, 3f));
                    break;
                case WarehouseGraph.NodeType.OutPort:
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.Position, new Vector3(0.1f, 0.1f, 0.2f));
                    break;
                case WarehouseGraph.NodeType.InPort:
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.Position, new Vector3(0.1f, 0.1f, 0.2f));
                    break;
            }
            
            Handles.Label(node.Position, node.ID.ToString());
        }
    }

    public void DrawPathGizmos(List<int> path)//绘制path路径的gizmos
    {
        if (_graph.Edges.Count == 0 || _graph.Edges.Count == 0)
            return;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var fromNode = _graph.GetNode(path[i]);
            var toNode = _graph.GetNode(path[i + 1]);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(fromNode.Position, toNode.Position);
        }
    }
    private float Heuristic(int nodeId, int targetNodeId)
    {
        var node = _graph.GetNode(nodeId);
        var targetNode = _graph.GetNode(targetNodeId);
        return Vector3.Distance(node.Position, targetNode.Position); // 使用欧几里得距离作为启发式
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
    
    public void GenerateTestGraph()//生成测试仓库图
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
                        _graph.AddDoubleEdge(pathPointIds[row, col], pathPointIds[row, col + 1], dist, true, 0.1f);
                    }
                    
                    // 垂直连接
                    if (row < 2)
                    {
                        float dist = Vector3.Distance(
                            _graph.Nodes.Find(n => n.ID == pathPointIds[row, col]).Position,
                            _graph.Nodes.Find(n => n.ID == pathPointIds[row + 1, col]).Position
                        );
                        _graph.AddDoubleEdge(pathPointIds[row, col], pathPointIds[row + 1, col], dist, true, 0.1f);
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
                _graph.AddDoubleEdge(id, nearestPathId, dist, true, 0.2f);
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
                _graph.AddDoubleEdge(id, connectPoint, dist, true, 0.3f);
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
                    _graph.AddDoubleEdge(id, nearestPathId, dist, true, 0.1f);
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
                _graph.AddDoubleEdge(inPortId, nearestInPath, inDist, true, 0.1f);
                
                // 出库口
                int outPortId = 4001;
                Vector3 outPos = new Vector3(50, 0, 40);
                _graph.AddNode(outPortId, WarehouseGraph.NodeType.OutPort, outPos, 0, float.MaxValue);
                
                // 连接到最近的路径点
                int nearestOutPath = pathPointIds[2, 2];
                float outDist = Vector3.Distance(outPos, _graph.Nodes.Find(n => n.ID == nearestOutPath).Position);
                _graph.AddDoubleEdge(outPortId, nearestOutPath, outDist, true, 0.1f);
            }
        }
        _graph.AddDoubleEdge(108, 1108, 10.0f, false, 0.1f);
        _graph.AddDoubleEdge(1108, 2108, 10.0f, false, 0.1f);
        _graph.AddDoubleEdge(2108, 3108, 10.0f, false, 0.1f);

    }
    public void GenerateTestGraph1()
    {
            // 清空现有图数据
            _graph = new WarehouseGraph();

        // 创建四层仓库图
        for (int floor = 1; floor <= 4; floor++)
        {
            // 添加当前层的25个节点（5x5网格）
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    // 计算节点ID：层数*1000 + (1-25)
                    int nodeId = floor * 1000 + (row * 5 + col + 1);
            
                    // 计算节点位置：x=列索引*10, y=层高*20, z=行索引*10
                    Vector3 position = new Vector3(col * 10, (floor - 1) * 20, row * 10);
            
                    // 添加节点（类型为PathPoint，承重限默认1000）
                    _graph.AddNode(nodeId, WarehouseGraph.NodeType.PathPoint, position, floor, 1000);
                }
            }

            // 创建当前层的内部连接（双向边）
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    int currentNodeId = floor * 1000 + (row * 5 + col + 1);
            
                    // 向右连接（如果不是最后一列）
                    if (col < 4)
                    {
                        int rightNodeId = floor * 1000 + (row * 5 + (col + 1) + 1);
                        _graph.AddDoubleEdge(currentNodeId, rightNodeId, 10, true, 0);
                    }
            
                    // 向下连接（如果不是最后一行）
                    if (row < 4)
                    {
                        int downNodeId = floor * 1000 + ((row + 1) * 5 + col + 1);
                        _graph.AddDoubleEdge(currentNodeId, downNodeId, 10, true, 0);
                    }
                }
            }

            // 连接当前层和上一层（从第二层开始）
            if (floor > 1)
            {
                int currentFirstNode = floor * 1000 + 1;
                int upperFirstNode = (floor - 1) * 1000 + 1;
                int currentFirstNodeID = floor * 1000 + 2;
                int upperFirstNodeID = (floor - 1) * 1000 + 2;
                _graph.AddDoubleEdge(currentFirstNode, upperFirstNode, 20, true, 0);
                _graph.AddDoubleEdge(currentFirstNodeID, upperFirstNodeID, 20, true, 0);
            }
        }

        for (int floor = 1; floor <= 4; floor++)
        {
            if (floor == 1)
            {
                _graph.AddNode(0, WarehouseGraph.NodeType.InPort, _graph.GetNode(1011).Position - new Vector3(10, 0, 0), floor, 1000f);
                _graph.AddNode(1, WarehouseGraph.NodeType.OutPort, _graph.GetNode(1015).Position + new Vector3(10, 0, 0), floor, 1000f);
                foreach (int nodeId in new int[] { 1001, 1006, 1011, 1016, 1021 })
                {
                    float distance = Vector3.Distance(_graph.GetNode(0).Position, _graph.GetNode(nodeId).Position);
                    _graph.AddDoubleEdge(0, nodeId, distance, false, 0);
                    float distance1 = Vector3.Distance(_graph.GetNode(0).Position, _graph.GetNode(nodeId + 4).Position);
                    _graph.AddDoubleEdge(1, nodeId + 4, distance1, false, 0);
                }
            }

            if (floor == 1 || floor == 3)
            {
                int chargeNodeID1 = floor * 1000 + 100 + 1;
                int chargeNodeID2 = floor * 1000 + 100 + 2;
                
                _graph.AddNode(chargeNodeID1, WarehouseGraph.NodeType.Charger, _graph.GetNode(floor * 1000 + 24).Position + new Vector3(0, 0, 10), floor, 1000f);
                _graph.AddDoubleEdge(chargeNodeID1, floor * 1000 + 24, 10, false, 0);
                _graph.AddNode(chargeNodeID2, WarehouseGraph.NodeType.Charger, _graph.GetNode(floor * 1000 + 22).Position + new Vector3(0, 0, 10), floor, 1000f);
                _graph.AddDoubleEdge(chargeNodeID2, floor * 1000 + 22, 10, false, 0);
            }
            
            int floorElevatorID1 = floor * 1000 + 1;
            int floorElevatorID2 = floor * 1000 + 2;
            _graph.GetNode(floorElevatorID1).Type = WarehouseGraph.NodeType.Elevator;
            _graph.GetNode(floorElevatorID2).Type = WarehouseGraph.NodeType.Elevator;
            
            foreach(int floorShelfID in new int[]{7, 17})
            {
                for (int j = 0; j < 3; j++)
                {
                    int shelfID = floor * 1000 + floorShelfID + j;
                    _graph.GetNode(shelfID).WeightLimit = floor * 20;
                    _graph.GetNode(shelfID).Type = WarehouseGraph.NodeType.Shelf;
                }
            }
        }


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
}

