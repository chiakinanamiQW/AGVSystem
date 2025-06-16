using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text TimeEstimationText;
    public Slider BatteryThresholdSlider;
    public InputField TaskInputField;

    void Start()
    {
        // 初始化UI组件
    }

    // 任务输入面板
  /*  public void OnCreateTask()
    {
        int source = int.Parse(TaskInputField.text.Split(',')[0]);
        int target = int.Parse(TaskInputField.text.Split(',')[1]);
        LogicLayer.TaskScheduler.CreateManualTask(source, target);
    }

    void Update()
    {
        // 实时数据更新
        foreach (var agv in DataLayer.AGVAgents)
        {
            // 计算并显示：
            // 1. 预计到达时间 = 剩余路径长度 / AGV速度
            // 2. 预估能耗 = 剩余距离 * 能耗系数
        }
    }*/
}
