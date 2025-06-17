using System.Collections.Generic;
using UnityEngine;

// 仓库图类：用于表示仓库中的节点和边的图结构
public class WarehouseGraph
{
    // 节点列表：存储仓库图中的所有节点信息
    public List<Node> Nodes;
    // 边列表：存储仓库图中节点之间的连接关系
    public List<Edge> Edges;

    // 构造函数：初始化节点列表和边列表
    public WarehouseGraph()
    {
        Nodes = new List<Node>();
        Edges = new List<Edge>();
    }

    // 添加节点到仓库图中
    // id: 节点的唯一标识符
    // type: 节点的类型（如货架、充电点等）
    // position: 节点在仓库场景中的三维坐标位置
    // floor: 节点所在的楼层编号
    // weightLimit: 节点（通常是货架）的最大承重限制
    public void AddNode(int id, NodeType type, Vector3 position, int floor, float weightLimit)
    {
        Nodes.Add(new Node
        {
            ID = id,
            Type = type,
            Position = position,
            Floor = floor,
            WeightLimit = weightLimit,
            IsBlocked = false
        });
    }

    // 添加边到仓库图中
    // fromNodeId: 边的起点节点ID
    // toNodeId: 边的终点节点ID
    // distance: 边连接的两个节点之间的物理距离
    // isBidirectional: 是否双向通行（true为双向，false为单向）
    // congestion: 拥堵系数（0到1之间），用于后续路径代价计算
    public void AddEdge(int fromNodeId, int toNodeId, float distance, bool isBidirectional, float congestion)
    {
        Edges.Add(new Edge
        {
            FromNode = fromNodeId,
            ToNode = toNodeId,
            Distance = distance,
            IsBidirectional = isBidirectional,
            Congestion = congestion
        });
    }

    // 更新节点的阻塞状态
    // nodeId: 要更新的节点ID
    // isBlocked: 设置节点为阻塞或非阻塞状态
    public void UpdateNodeStatus(int nodeId, bool isBlocked)
    {
        var node = Nodes.Find(n => n.ID == nodeId);
        if (node != null)
        {
            node.IsBlocked = isBlocked;
        }
    }

    // 节点类：表示仓库中的一个节点（如货架、出入口、充电点等）
    public class Node
    {
        // 唯一标识该节点的ID
        public int ID;                      // 节点唯一标识符
        // 节点类型，用于区分不同功能的节点
        public NodeType Type;               // 节点类型：货架、电梯、出入口、充电点等
        // 节点在仓库中的三维坐标位置
        public Vector3 Position;            // 节点在3D空间中的位置坐标
        // 节点所在的楼层编号
        public int Floor;                   // 节点所在的楼层编号
        // 最大承重限制，通常用于货架节点
        public float WeightLimit;           // 节点对应货架的最大承重
        // 是否阻塞，表示该节点当前是否不可通行
        public bool IsBlocked;              // 是否为故障/不可通行状态
    }

    // 边类：表示仓库中两个节点之间的连接关系
    public class Edge
    {
        // 边的起点节点ID
        public int FromNode;                // 起点节点ID
        // 边的终点节点ID
        public int ToNode;                  // 终点节点ID
        // 起点和终点节点之间的距离
        public float Distance;              // 节点之间的物理距离
        // 是否双向通行（true表示该边可双向通行）
        public bool IsBidirectional;        // 是否为双向通行道路
        // 拥堵系数：表示路径当前的拥堵程度（值范围通常为0到1）
        public float Congestion;            // 拥堵系数（0-1），用于路径代价计算
    }

    // 节点类型枚举：定义仓库图中节点的不同类型
    public enum NodeType
    {
        Shelf,      // 货架
        Elevator,   // 垂直电梯
        InPort,     // 入库口
        OutPort,    // 出库口
        Charger     // 充电点
    }
}