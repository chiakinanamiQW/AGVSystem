using System.Collections.Generic;
using UnityEngine;
using static TaskScheduler;

public class WarehouseGraph
{
    public List<Node> Nodes;
    public List<Edge> Edges;

    public WarehouseGraph()
    {
        Nodes = new List<Node>();
        Edges = new List<Edge>();
    }

    public void AddNode(int id, NodeType type, Vector3 position, int floor, float weightLimit)//添加node
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

    public void AddEdge(int fromNodeId, int toNodeId, float distance, bool isBidirectional, float congestion)//添加edge
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

    public void AddDoubleEdge(int NodeId1, int NodeId2, float distance, bool isBidirectional, float congestion)//添加双向edge,注意该函数添加的两边属性一样
    {
        Edges.Add(new Edge
        {
            FromNode = NodeId1,
            ToNode = NodeId2,
            Distance = distance,
            IsBidirectional = isBidirectional,
            Congestion = congestion
        });
        
        Edges.Add(new Edge
        {
            FromNode = NodeId2,
            ToNode = NodeId1,
            Distance = distance,
            IsBidirectional = isBidirectional,
            Congestion = congestion
        });
    }

    public Node GetNode(int id)//通过nodeId获取Node实例
    {
        return Nodes.Find(n => n.ID == id);
    }
    
    public void UpdateNodeStatus(int nodeId, bool isBlocked)
    {
        var node = Nodes.Find(n => n.ID == nodeId);
        if (node != null)
        {
            node.IsBlocked = isBlocked;
        }
    }
    public class Node
    {
        public int ID;
        public NodeType Type;
        public Vector3 Position;
        public int Floor;
        public float WeightLimit;
        public bool IsBlocked;
    }

    public class Edge
    {
        public int FromNode;
        public int ToNode;
        public float Distance;
        public bool IsBidirectional;
        public float Congestion;   // 拥堵系数 0-1
    }

    public enum NodeType
    {
        Shelf,
        Elevator,
        InPort,
        OutPort,
        Charger,
        PathPoint
    }
  
}
public class AGVAgent
{
    public int AGVID;
    public int CurrentNode;
    public float BatteryLevel = 100f;
    public List<int> currentPath;
    public TransportTask CurrentTask;
    public float loads;
}
