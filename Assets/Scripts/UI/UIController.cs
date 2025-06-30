using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TaskInputHandler taskInputHandler;

    void Start()
    {
        // 确保TaskScheduler实例存在
        var scheduler = TaskScheduler.Instance;
    }


public void OnCreateTaskClicked()
    {
        taskInputHandler.OnSubmitTask();
        
    }

    
}