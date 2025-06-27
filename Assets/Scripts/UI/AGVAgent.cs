using UnityEngine;

public class AGVAgent : MonoBehaviour
{
    public int AGVID = 1;                  // AGV唯一ID
    public int CurrentNode = -1;           // 当前节点ID
    public float BatteryLevel = 100f;      // 电量（0-100）
    public float loads = 0f;               // 载重（kg）

    // 模拟电量消耗和移动
    void Update()
    {
        // 模拟电量缓慢下降
        BatteryLevel = Mathf.Max(0, BatteryLevel - 0.1f);

        // 模拟随机载重变化
        if (Random.value < 0.01f)
        {
            loads = Random.Range(0f, 50f);
        }

        // 模拟随机切换节点（每10秒切换一次）
        if (Time.time % 10 < 0.1f)
        {
            CurrentNode = Random.Range(1, 10); // 随机节点ID 1-9
        }
    }
}