using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Michsky.MUIP;

[System.Serializable]
public class OrderLineTemp
{
    public int productId;
    public string productName;
    public int quantity;
}

public class CartLine : MonoBehaviour
{
    [Header("Normal Mode Objects")]
    [SerializeField] private GameObject[] normalModeObjects;
    [Header("Edit Mode Objects")]
    [SerializeField] private GameObject[] editModeObjects;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI lineText; 
    [SerializeField] private TMP_InputField editQtyInput;
    private NotificationManager errorNotification;

    private AddOrderPopup parentPopup;
    private OrderLineTemp line;
    private bool isEditMode;

    public void Init(AddOrderPopup popup, OrderLineTemp lineData)
    {
        parentPopup = popup;
        line = lineData;
        errorNotification = popup.GetErrorNotification();

        RefreshUI();
        SetEditMode(false);
    }

    void RefreshUI()
    {
        lineText.text = $"[{line.productId}] {line.productName} x {line.quantity}";
    }

    void SetEditMode(bool edit)
    {
        isEditMode = edit;
        foreach (var obj in normalModeObjects) obj.SetActive(!edit);
        foreach (var obj in editModeObjects) obj.SetActive(edit);

        if (edit)
        {
            editQtyInput.text = line.quantity.ToString();
        }
    }

    public void OnEditClick()
    {
        SetEditMode(true);
    }

    public void OnDeleteClick()
    {
        parentPopup.RemoveLine(line);
    }

    public void OnSaveLineClick()
    {
        string qStr = editQtyInput.text.Trim();
        if (!int.TryParse(qStr, out int newQ) || newQ <= 0)
        {
            ShowError("Некорректное количество. Должно быть > 0");
            return;
        }

        line.quantity = newQ;
        RefreshUI();
        SetEditMode(false);
        parentPopup.OnLineQuantityChanged(line);
    }

    public void OnCancelEditClick()
    {
        SetEditMode(false);
    }

    private void ShowError(string msg)
    {
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
