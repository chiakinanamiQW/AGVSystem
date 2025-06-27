using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AGVStatusUI : MonoBehaviour
{
    // 引用UI文本组件
    public Text agvIDText;
    public Text nodeIDText;
    public Text batteryText;
    public Text loadText;

    // 引用AGV对象
    public GameObject agvObject;
    private AGVAgent agvAgent;

    void Start()
    {
        // 初始化时获取AGV组件
        if (agvObject != null)
        {
            agvAgent = agvObject.GetComponent<AGVAgent>();
        }
    }

    void Update()
    {
        // 确保AGV组件存在
        if (agvAgent == null) return;

        // 更新UI文本
        agvIDText.text = "AGV ID: " + agvAgent.AGVID;
        nodeIDText.text = "节点ID: " + agvAgent.CurrentNode;
        batteryText.text = "电量: " + agvAgent.BatteryLevel.ToString("F1") + "%";
        loadText.text = "载重: " + agvAgent.loads.ToString("F1") + "kg";
    }
}
