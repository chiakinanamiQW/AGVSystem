using UnityEngine;

public class AGVAgent : MonoBehaviour
{
    public int AGVID = 1;                  // AGVΨһID
    public int CurrentNode = -1;           // ��ǰ�ڵ�ID
    public float BatteryLevel = 100f;      // ������0-100��
    public float loads = 0f;               // ���أ�kg��

    // ģ��������ĺ��ƶ�
    void Update()
    {
        // ģ����������½�
        BatteryLevel = Mathf.Max(0, BatteryLevel - 0.1f);

        // ģ��������ر仯
        if (Random.value < 0.01f)
        {
            loads = Random.Range(0f, 50f);
        }

        // ģ������л��ڵ㣨ÿ10���л�һ�Σ�
        if (Time.time % 10 < 0.1f)
        {
            CurrentNode = Random.Range(1, 10); // ����ڵ�ID 1-9
        }
    }
}