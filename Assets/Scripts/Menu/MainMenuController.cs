using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roleText;

    [SerializeField] private GameObject productsButton;
    [SerializeField] private GameObject categoriesButton;
    [SerializeField] private GameObject ordersButton;
    [SerializeField] private GameObject warehouseButton;
    [SerializeField] private GameObject discountsButton;
    [SerializeField] private GameObject reviewsButton;
    [SerializeField] private GameObject logsButton;

    public void LogIn()
    {
        string role = DatabaseManager.Instance.GetRole();
        Debug.Log("Текущая роль: " + role);

        string roleNameRus = "неизвестно";
        if (role == "store_admin") roleNameRus = "администратор";
        else if (role == "store_manager") roleNameRus = "менеджер";
        else if (role == "store_seller") roleNameRus = "продавец";
        else if (role == "store_moderator") roleNameRus = "модератор";

       //roleText.text = "Ваш уровень доступа: " + roleNameRus;
       
        productsButton.SetActive(false);
        categoriesButton.SetActive(false);
        ordersButton.SetActive(false);
        warehouseButton.SetActive(false);
        discountsButton.SetActive(false);
        reviewsButton.SetActive(false);
        logsButton.SetActive(false);
        
        if (role == "store_admin")
        {
            productsButton.SetActive(true);
            categoriesButton.SetActive(true);
            ordersButton.SetActive(true);
            warehouseButton.SetActive(false);
            discountsButton.SetActive(true);
            reviewsButton.SetActive(true);
            logsButton.SetActive(true);
        }
        else if (role == "store_manager")
        {
            productsButton.SetActive(true);
            categoriesButton.SetActive(true);
            ordersButton.SetActive(true);
            warehouseButton.SetActive(false);
            discountsButton.SetActive(true);
        }
        else if (role == "store_seller")
        {
            productsButton.SetActive(true);
            ordersButton.SetActive(true);
        }
        else if (role == "store_moderator")
        {
            productsButton.SetActive(true);
            reviewsButton.SetActive(true);
        }
    }

    public void OnExitClick()
    {
        DatabaseManager.Instance.CloseConnection();
    }
}
