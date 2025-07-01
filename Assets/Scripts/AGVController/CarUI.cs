using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarUI : MonoBehaviour
{
    [Header("UI Text Components")]
    public Text electricText;           // ��ǰ����
    public Text currentNodeText;        // ��ǰ�ڵ�
    public Text agvIdText;             // AGV ID
    public Text loadsText;             // �ػ���
    public Text isMovingText;          // �ƶ�״̬
    public Text estimatedEnergyText;   // Ԥ���ܺ�
    public Text estimatedTimeText;     // Ԥ��ʱ��
    public Text pathProgressText;      // ·������
    public Text tasksCountText;        // ��������

    [Header("Energy Bar (Optional)")]
    public Slider energySlider;        // ������
    public Image energyFillImage;      // ���������ͼ��

    private CarController carController;

    void Start()
    {
        // ��ȡCarControllerʵ��
        carController = CarController.Instance;

        // ��ʼ��������
        if (energySlider != null)
        {
            energySlider.maxValue = 100f; // ������������100
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
        // ���µ�����ʾ
        if (electricText != null)
        {
            electricText.text = "����: " + carController.electric.ToString("F1") + " / 100";

            // ���ݵ����ı���ɫ
            if (carController.electric < 20f)
                electricText.color = Color.red;
            else if (carController.electric < 50f)
                electricText.color = Color.yellow;
            else
                electricText.color = Color.green;
        }

        // ���µ�����
        if (energySlider != null)
        {
            energySlider.value = carController.electric;

            // �ı��������ɫ
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

        // ���µ�ǰ�ڵ�
        if (currentNodeText != null)
        {
            currentNodeText.text = "��ǰ�ڵ�: " + carController.GetCurrentNodeId().ToString();
        }

        // ����AGV��Ϣ
        if (carController._agv != null)
        {
            if (agvIdText != null)
                agvIdText.text = "AGV ID: " + carController._agv.AGVID.ToString();

            if (loadsText != null)
                loadsText.text = "�ػ���: " + carController._agv.Loads.ToString();
        }

        // �����ƶ�״̬
        if (isMovingText != null)
        {
            bool isMoving = false;
            if (carController.path != null && carController.path.Count > 0)
            {
                isMoving = carController.currentPathIndex < carController.path.Count && carController.electric > 0;
            }
            isMovingText.text = "״̬: " + (isMoving ? "�ƶ���" : "ֹͣ");
            isMovingText.color = isMoving ? Color.green : Color.red;
        }

        // ����Ԥ����Ϣ
        if (estimatedEnergyText != null)
        {
            estimatedEnergyText.text = "Ԥ���ܺ�: " + carController.EstimatedEnergyConsumption.ToString("F1");
        }

        if (estimatedTimeText != null)
        {
            estimatedTimeText.text = "Ԥ��ʱ��: " + carController.EstimatedTimeToComplete.ToString("F1") + "s";
        }

        // ����·������
        if (pathProgressText != null && carController.path != null && carController.path.Count > 0)
        {
            int current = carController.currentPathIndex;
            int total = carController.path.Count;
            pathProgressText.text = "·������: " + current.ToString() + "/" + total.ToString();
        }

        // ������������
        if (tasksCountText != null)
        {
            tasksCountText.text = "��������: " + carController.Tasks.Count.ToString();
        }
    }
}