using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    private static CarController _instance = null;

    public static CarController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CarController>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CarController");
                    _instance = obj.AddComponent<CarController>();
                    DontDestroyOnLoad(obj); // 可选：跨场景保持
                }
            }
            return _instance;
        }
    }


    private void Awake()
    {
        if (_agv == null)
        {
            _agv = new AGVAgent
            {
                AGVID = 1,
                CurrentNode = startIndex,
                Loads = 0
            };
        }
    }



    private CarController()
    {

    }

    private PathfindingService _pathfindingServe = PathfindingService.Instance;
    private WarehouseGraph graph;
    private List<int> _path = new List<int>();
    public AGVAgent _agv;
    private int times = 0;
    public int startIndex = 1;
    public int currentIndex;
    public List<TransportTask> Tasks = new List<TransportTask>();

    [Header("Energy Settings")]
    [SerializeField] private float maxElectric = 100f; // 最大电量
    [SerializeField] private float energyConsumptionPerSecond = 0.5f;  // 每秒消耗的电量
    [SerializeField] private float energyThreshold = 20f;
    public float electric; // 当前电量

    private List<int> charge1 = new List<int>();
    private List<int> charge2 = new List<int>();
    private List<int> charge3 = new List<int>();
    private List<int> charge4 = new List<int>();



    public float EstimatedEnergyConsumption; // 预估总能耗

    public float EstimatedTimeToComplete; // 预估完成时间(秒)

    // 计算路径的预估能耗和完成时间
    private void CalculatePathEstimations(List<int> nodeIndices)
    {
        if (nodeIndices == null || nodeIndices.Count < 2 || graph == null)
        {
            EstimatedEnergyConsumption = 0f;
            EstimatedTimeToComplete = 0f;
            return;
        }

        // 计算总距离
        float totalDistance = 0f;
        for (int i = 0; i < nodeIndices.Count - 1; i++)
        {
            Vector3 startPos = graph.GetNode(nodeIndices[i]).Position;
            Vector3 endPos = graph.GetNode(nodeIndices[i + 1]).Position;
            totalDistance += Vector3.Distance(startPos, endPos);
        }

        // 计算预估时间 = 总距离 / 移动速度
        EstimatedTimeToComplete = totalDistance / moveSpeed;

        // 计算预估能耗 = 每秒能耗 * 预估时间
        EstimatedEnergyConsumption = energyConsumptionPerSecond * EstimatedTimeToComplete;

        Debug.Log($"路径预估 - 距离: {totalDistance}m, 时间: {EstimatedTimeToComplete:F1}s, 能耗: {EstimatedEnergyConsumption:F1}单位");
    }



    private void Start()
    {
        _pathfindingServe = PathfindingService.Instance;
        graph = _pathfindingServe._graph;
        electric = maxElectric; // 初始化电量
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

        if (times > 0)
        {
            _path = _pathfindingServe.FindPath(currentIndex, end, _agv);
            times++;
            Debug.Log(_path);
        }
        else
        {
            _path = _pathfindingServe.FindPath(startIndex, end, _agv);
            Debug.Log(_path[0]);
            times++;
        }
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
        CalculatePathEstimations(nodeIndices);
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
    public bool calculated = true;
    void Update()
    {
        if (graph == null) { Debug.Log("graph为空"); }
        if (!isMoving || path.Count == 0) return;


        if (electric > 0 && isMoving)
        {
            float energyConsumed = energyConsumptionPerSecond * Time.deltaTime;
            electric -= energyConsumed;
            electric = Mathf.Max(0, electric); // 防止电量变负

            Debug.Log($"Current Energy: {electric:F1}");
        }
        if (electric < energyThreshold && calculated)
        {
            charge1 = _pathfindingServe.FindPath(currentIndex, 1101, _agv);
            charge2 = _pathfindingServe.FindPath(currentIndex, 1102, _agv);
            charge3 = _pathfindingServe.FindPath(currentIndex, 3101, _agv);
            charge4 = _pathfindingServe.FindPath(currentIndex, 3102, _agv);
            int minCount = charge1.Count;
            int target = 1101;
            if (charge2.Count < minCount)
            {
                minCount = charge2.Count;
                target = 1102;
            }
            if (charge3.Count < minCount)
            {
                minCount = charge2.Count;
                target = 3101;
            }
            if (charge4.Count < minCount)
            {
                minCount = charge2.Count;
                target = 3102;
            }
            calculated = false;
            TaskScheduler.Instance.CreateChargeTask(currentIndex, target);
        }
        // 电量耗尽时停止移动
        if (electric <= 0)
        {
            isMoving = false;
            Debug.LogWarning("电量耗尽！小车已停止。");
        }
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
        Debug.LogWarning("Path completed!");
        TaskScheduler.Instance.OnTaskCompleted(_agv);

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

    //hsh的ui

    public void Start()
    {
     public Text t;
    t.text = "电量：" ;
    }


}

