using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AGVStatusUI : MonoBehaviour
{
    // ����UI�ı����
    public Text agvIDText;
    public Text nodeIDText;
    public Text batteryText;
    public Text loadText;

    // ����AGV����
    public GameObject agvObject;
    private AGVAgent agvAgent;

    void Start()
    {
        // ��ʼ��ʱ��ȡAGV���
        if (agvObject != null)
        {
            agvAgent = agvObject.GetComponent<AGVAgent>();
        }
    }

    void Update()
    {
        // ȷ��AGV�������
        if (agvAgent == null) return;

        // ����UI�ı�
        agvIDText.text = "AGV ID: " + agvAgent.AGVID;
        nodeIDText.text = "�ڵ�ID: " + agvAgent.CurrentNode;
        batteryText.text = "����: " + agvAgent.BatteryLevel.ToString("F1") + "%";
        loadText.text = "����: " + agvAgent.loads.ToString("F1") + "kg";
    }
}
