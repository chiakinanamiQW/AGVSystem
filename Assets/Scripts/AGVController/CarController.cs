using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private static CarController _instance = null;

    public static CarController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CarController();
            }

            return _instance;
        }
    }
    public WarehouseGraph _graph;

    private CarController(WarehouseGraph graph)
    {
        _graph = graph;
    }

    private CarController()
    {

    }

    private PathfindingService _pathfindingServe = PathfindingService.Instance;
    private WarehouseGraph graph;
    private  List<int> _path = new List<int>();
    private AGVAgent _agv;
    
    public int startIndex = 1;
    public int currentIndex;


    private void Start()
    {
        _pathfindingServe = PathfindingService.Instance;
        graph = _pathfindingServe._graph;
    }
   
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private float rotationSpeed = 5f; // 旋转速度
    [SerializeField] private float reachedDistance = 0.1f; // 到达节点的判定距离

    private List<int> pathNodeIndices = new List<int>();
    private List<Vector3> path = new List<Vector3>(); // 存储路径位置
    public int currentPathIndex = 0; // 当前路径点索引
    private bool isMoving = false; // 是否正在移动
    
    // 设置路径并开始移动
    public void CarMove(int end)
    {
        _path = _pathfindingServe.FindPath(startIndex, end, _agv);
/*        this.gameObject.transform.position = graph.GetNode(startIndex).Position;*/
        SetPath(_path);
    }
    private void SetPath(List<int> nodeIndices)
    {
        if (nodeIndices == null || nodeIndices.Count == 0 || graph == null)
            return;

        // 1. 保存节点 ID 列表
        pathNodeIndices = new List<int>(nodeIndices);

        // 2. 把节点位置转换成 world-space 路径
        path.Clear();
        foreach (int id in pathNodeIndices)
        {
            var node = graph.GetNode(id);
            if (node != null)
                path.Add(node.Position);
        }

        // 3. 重置行进状态
        currentPathIndex = 0;
        isMoving = true;

        // 4. 初始化 currentIndex——此时 AGV 位于路径第一个节点上
        currentIndex = pathNodeIndices[0];
    }

    void Update()
    {
        if (!isMoving || path.Count == 0) return;

        // 获取当前目标点
        Vector3 targetPosition = path[currentPathIndex];


        // 移动物体
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // 旋转朝向目标方向
        if (transform.position != targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        if (Vector3.Distance(transform.position, targetPosition) <= reachedDistance)
        {
            // 更新 currentIndex 为刚到达的节点
            currentIndex = pathNodeIndices[currentPathIndex];

            currentPathIndex++;
            if (currentPathIndex >= path.Count)
            {
                isMoving = false;
                OnPathCompleted();
            }
        }
    }
    public int GetCurrentNodeId()
    {
        return currentIndex;
    }
    // 路径完成时的回调
    private void OnPathCompleted()
    {
        Debug.Log("Path completed!");
        // 这里可以添加路径完成后的逻辑
    }

    // 绘制路径Gizmos（可选，用于调试）
    private void OnDrawGizmos()
    {
        if (path == null || path.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }

        // 绘制当前目标点
        if (isMoving && currentPathIndex < path.Count)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(path[currentPathIndex], 0.2f);
        }
    }
}
