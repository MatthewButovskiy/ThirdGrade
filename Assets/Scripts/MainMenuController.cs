using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject managerPanelButton;
    [SerializeField] private GameObject sellerPanelButton;
    [SerializeField] private GameObject moderatorPanelButton;

    void Start()
    {
        string role = DatabaseManager.Instance.GetRole();
        Debug.Log("Текущая роль: " + role);

        // Сначала всё отключим
        managerPanelButton.SetActive(false);
        sellerPanelButton.SetActive(false);
        moderatorPanelButton.SetActive(false);

        // Включаем в зависимости от роли
        if (role == "store_manager")
        {
            managerPanelButton.SetActive(true);
        }
        else if (role == "store_seller")
        {
            sellerPanelButton.SetActive(true);
        }
        else if (role == "store_moderator")
        {
            moderatorPanelButton.SetActive(true);
        }
    }

    public void OnExitClick()
    {
        // Показываем алерт или нотификацию, что выходим, потом закрываем соединение и выходим из приложения
        DatabaseManager.Instance.CloseConnection();
        //Application.Quit(); // В редакторе не выйдет, но в сборке выйдет.
    }
}