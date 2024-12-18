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

        string roleNameRus = "Неизвестно";
        if (role == "store_admin") roleNameRus = "Администратор";
        else if (role == "store_manager") roleNameRus = "Менеджер";
        else if (role == "store_seller") roleNameRus = "Продавец";
        else if (role == "store_moderator") roleNameRus = "Модератор";

        roleText.text = "Вы вошли как: " + roleNameRus;

        // Сначала отключаем все кнопки
        productsButton.SetActive(false);
        categoriesButton.SetActive(false);
        ordersButton.SetActive(false);
        warehouseButton.SetActive(false);
        discountsButton.SetActive(false);
        reviewsButton.SetActive(false);
        logsButton.SetActive(false);

        // Включаем в зависимости от роли
        if (role == "store_admin")
        {
            productsButton.SetActive(true);
            categoriesButton.SetActive(true);
            ordersButton.SetActive(true);
            warehouseButton.SetActive(true);
            discountsButton.SetActive(true);
            reviewsButton.SetActive(true);
            logsButton.SetActive(true);
        }
        else if (role == "store_manager")
        {
            productsButton.SetActive(true);
            categoriesButton.SetActive(true);
            ordersButton.SetActive(true);
            warehouseButton.SetActive(true);
            discountsButton.SetActive(true);
        }
        else if (role == "store_seller")
        {
            productsButton.SetActive(true);
            ordersButton.SetActive(true);
        }
        else if (role == "store_moderator")
        {
            productsButton.SetActive(true); // read-only
            reviewsButton.SetActive(true);
        }
    }

    public void OnExitClick()
    {
        DatabaseManager.Instance.CloseConnection();
    }

    // Далее вы можете создать методы OnProductsClick(), OnCategoriesClick() и т.д.
    // При нажатии на конкретную кнопку можно открывать соответствующую сцену или панель.
}
