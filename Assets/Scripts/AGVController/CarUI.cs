using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarUI : MonoBehaviour
{
    [Header("UI Text Components")]
    public Text electricText;           // 当前电量
    public Text currentNodeText;        // 当前节点
    public Text agvIdText;             // AGV ID
    public Text loadsText;             // 载货量
    public Text isMovingText;          // 移动状态
    public Text estimatedEnergyText;   // 预估能耗
    public Text estimatedTimeText;     // 预估时间
    public Text pathProgressText;      // 路径进度
    public Text tasksCountText;        // 任务数量

    [Header("Energy Bar (Optional)")]
    public Slider energySlider;        // 电量条
    public Image energyFillImage;      // 电量条填充图像

    private CarController carController;

    void Start()
    {
        // 获取CarController实例
        carController = CarController.Instance;

        // 初始化电量条
        if (energySlider != null)
        {
            energySlider.maxValue = 100f; // 假设最大电量是100
        }
    }

    void Update()
    {
        if (carController == null)
        {
            carController = CarController.Instance;
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // 更新电量显示
        if (electricText != null)
        {
            electricText.text = "电量: " + carController.electric.ToString("F1") + " / 100";

            // 根据电量改变颜色
            if (carController.electric < 20f)
                electricText.color = Color.red;
            else if (carController.electric < 50f)
                electricText.color = Color.yellow;
            else
                electricText.color = Color.green;
        }

        // 更新电量条
        if (energySlider != null)
        {
            energySlider.value = carController.electric;

            // 改变电量条颜色
            if (energyFillImage != null)
            {
                if (carController.electric < 20f)
                    energyFillImage.color = Color.red;
                else if (carController.electric < 50f)
                    energyFillImage.color = Color.yellow;
                else
                    energyFillImage.color = Color.green;
            }
        }

        // 更新当前节点
        if (currentNodeText != null)
        {
            currentNodeText.text = "当前节点: " + carController.GetCurrentNodeId().ToString();
        }

        // 更新AGV信息
        if (carController._agv != null)
        {
            if (agvIdText != null)
                agvIdText.text = "AGV ID: " + carController._agv.AGVID.ToString();

            if (loadsText != null)
                loadsText.text = "载货量: " + carController._agv.Loads.ToString();
        }

        // 更新移动状态
        if (isMovingText != null)
        {
            bool isMoving = false;
            if (carController.path != null && carController.path.Count > 0)
            {
                isMoving = carController.currentPathIndex < carController.path.Count && carController.electric > 0;
            }
            isMovingText.text = "状态: " + (isMoving ? "移动中" : "停止");
            isMovingText.color = isMoving ? Color.green : Color.red;
        }

        // 更新预估信息
        if (estimatedEnergyText != null)
        {
            estimatedEnergyText.text = "预估能耗: " + carController.EstimatedEnergyConsumption.ToString("F1");
        }

        if (estimatedTimeText != null)
        {
            estimatedTimeText.text = "预估时间: " + carController.EstimatedTimeToComplete.ToString("F1") + "s";
        }

        // 更新路径进度
        if (pathProgressText != null && carController.path != null && carController.path.Count > 0)
        {
            int current = carController.currentPathIndex;
            int total = carController.path.Count;
            pathProgressText.text = "路径进度: " + current.ToString() + "/" + total.ToString();
        }

        // 更新任务数量
        if (tasksCountText != null)
        {
            tasksCountText.text = "任务数量: " + carController.Tasks.Count.ToString();
        }
    }
}