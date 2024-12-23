using UnityEngine;
using System;

public class DeleteConfirmation : MonoBehaviour
{
    private Action onConfirmAction;
    private Action onCancelAction;

    // Инициализация подтверждения
    public void Initialize(Action confirmAction, Action cancelAction = null)
    {
        onConfirmAction = confirmAction;
        onCancelAction = cancelAction;
    }

    // При подтверждении
    public void OnConfirmClick()
    {
        onConfirmAction?.Invoke();
        gameObject.SetActive(false);
    }

    // При отмене
    public void OnCancelClick()
    {
        onCancelAction?.Invoke();
        gameObject.SetActive(false);
    }
}