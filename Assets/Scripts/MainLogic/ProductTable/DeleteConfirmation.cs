using UnityEngine;
using System;

public class DeleteConfirmation : MonoBehaviour
{
    private Action onConfirmAction;
    private Action onCancelAction;
    
    public void Initialize(Action confirmAction, Action cancelAction = null)
    {
        onConfirmAction = confirmAction;
        onCancelAction = cancelAction;
    }
    
    public void OnConfirmClick()
    {
        onConfirmAction?.Invoke();
        gameObject.SetActive(false);
    }
    
    public void OnCancelClick()
    {
        onCancelAction?.Invoke();
        gameObject.SetActive(false);
    }
}