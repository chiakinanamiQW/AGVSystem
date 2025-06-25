using System.Collections.Generic;
using UnityEngine;
using static TaskScheduler;

public class WarehouseGraph
{
    // 节点列表：表示仓库中的所有节点（如货架、充电点、电梯等）
    public List<Node> Nodes;
    // 边列表：表示仓库中的所有连接关系（如巷道、通道等）
    public List<Edge> Edges;

    // 构造函数：初始化节点列表和边列表
    public WarehouseGraph()
    {
        Nodes = new List<Node>();
        Edges = new List<Edge>();
    }

    // 添加一个节点到仓库图中
    // id: 节点的唯一标识符
    // type: 节点的类型（例如货架、充电点、电梯等）
    // position: 节点的三维位置坐标
    // floor: 节点所在的楼层
    // weightLimit: 节点的承重限制
    public void AddNode(int id, NodeType type, Vector3 position, int floor, float weightLimit)
    {
        Nodes.Add(new Node
        {
            ID = id,                     // 节点唯一标识符
            Type = type,                 // 节点类型
            Position = position,         // 节点位置
            Floor = floor,               // 节点所在楼层
            WeightLimit = weightLimit,   // 节点承重限制
            Weight = 0.0f,               //默认现承载重量为零
            IsBlocked = false            // 默认节点不被阻塞
        });
    }

    // 添加一个边到仓库图中
    // fromNodeId: 边的起点节点ID
    // toNodeId: 边的终点节点ID
    // distance: 边的距离（或耗时）
    // isBidirectional: 是否为双向道路
    // congestion: 拥堵系数，0表示无拥堵，1表示完全拥堵
    public void AddEdge(int fromNodeId, int toNodeId, float distance, bool isBidirectional, float congestion)
    {
        Edges.Add(new Edge
        {
            FromNode = fromNodeId,         // 边的起点节点ID
            ToNode = toNodeId,             // 边的终点节点ID
            Distance = distance,           // 边的距离
            IsBidirectional = isBidirectional, // 是否双向通行
            Congestion = congestion        // 拥堵系数
        });
    }

    // 添加双向边：创建一对相反方向的边，起点和终点之间的连接
    public void AddDoubleEdge(int NodeId1, int NodeId2, float distance, bool isBidirectional, float congestion)
    {
        // 添加NodeId1到NodeId2的边
        Edges.Add(new Edge
        {
            FromNode = NodeId1,
            ToNode = NodeId2,
            Distance = distance,
            IsBidirectional = isBidirectional,
            Congestion = congestion
        });

        // 添加NodeId2到NodeId1的边（双向）
        Edges.Add(new Edge
        {
            FromNode = NodeId2,
            ToNode = NodeId1,
            Distance = distance,
            IsBidirectional = isBidirectional,
            Congestion = congestion
        });
    }

    // 通过节点ID获取对应的节点实例
    // id: 节点的唯一标识符
    public Node GetNode(int id)
    {
        return Nodes.Find(n => n.ID == id); // 查找并返回与id匹配的节点
    }

    // 更新节点的状态（是否阻塞）
    // nodeId: 要更新状态的节点ID
    // isBlocked: 设置该节点为阻塞或非阻塞
    public void UpdateNodeStatus(int nodeId, bool isBlocked)
    {
        var node = Nodes.Find(n => n.ID == nodeId);
        if (node != null)
        {
            node.IsBlocked = isBlocked; // 设置节点为阻塞或解除阻塞
        }
    }

    // 节点类：表示仓库中的一个节点（如货架、电梯、充电点等）
    [System.Serializable]
    public class Node
    {
        public int ID;                      // 节点唯一标识符
        public NodeType Type;               // 节点类型：货架、电梯、充电点等
        public Vector3 Position;            // 节点的三维位置坐标
        public int Floor;                   // 节点所在的楼层编号
        public float WeightLimit;           // 节点的承重限制（用于货架等）
        public float Weight;                //节点现承载重量（用于货架)
        public bool IsBlocked;              // 节点是否被阻塞（如故障）
    }

    // 边类：表示仓库中两个节点之间的连接关系
    public class Edge
    {
        public int FromNode;                // 起点节点ID
        public int ToNode;                  // 终点节点ID
        public float Distance;              // 节点之间的物理距离
        public bool IsBidirectional;        // 是否双向通行
        public float Congestion;            // 拥堵系数（0-1，表示路径的拥堵程度）
    }

    // 节点类型枚举：定义仓库图中节点的不同类型
    public enum NodeType
    {
        Shelf,      // 货架
        Elevator,   // 垂直电梯
        InPort,     // 入库口
        OutPort,    // 出库口
        Charger,    // 充电点
        PathPoint   // 路径点（用于路径规划中）
    }
}

// AGV代理类：表示自动引导车（AGV）在仓库中的状态
public partial class AGVAgent//小车
{
    public int AGVID;                   // AGV的唯一标识符
    public int CurrentNode;             // AGV当前所在的节点ID
    public float BatteryLevel = 100f;   // AGV的电池电量，默认为100%
    public List<int> CurrentPath;       // AGV当前的路径，由节点ID组成的列表
    public TransportTask CurrentTask;   // AGV当前正在执行的任务
    public float Loads;                 // AGV的载重（表示AGV正在承载的货物重量）
}