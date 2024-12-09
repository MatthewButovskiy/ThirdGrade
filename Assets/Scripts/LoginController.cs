using Michsky.MUIP;
using UnityEngine;
using Npgsql;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [SerializeField] private CustomInputField loginField;
    [SerializeField] private CustomInputField passwordField;
    [SerializeField] private NotificationManager errorNotification; 
    [SerializeField] private GameObject startArea;

    private string server = "172.20.7.54";
    private string port = "5432";
    private string database = "db2093_02";

    public void OnLoginClick()
    {
        string login = loginField.inputText.text.Trim();
        string password = passwordField.inputText.text.Trim();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            ShowError("Пожалуйста, введите логин и пароль.");
            return;
        }

        string connectionString = $"Server={server};Port={port};User Id={login};Password={password};Database={database}";

        // Пробуем просто соединиться (без using, т.к. не хотим закрывать его сразу)
        NpgsqlConnection testConn = new NpgsqlConnection(connectionString);
        try
        {
            testConn.Open();
            Debug.Log("Подключение к базе данных успешно!");
            Debug.Log($"Logged in as {login}");
            testConn.Close();

            // Если получилось открыть и закрыть testConn - значит креды верны
            // Теперь установим эти креды в DatabaseManager и откроем постоянное соединение.
            DatabaseManager.Instance.SetConnectionString(connectionString);
            if (DatabaseManager.Instance.OpenConnection())
            {
                DatabaseManager.Instance.SetRole(login);
                gameObject.SetActive(false);
                startArea.SetActive(true);
                // Перейдём на сцену главного меню или активируем UI
                // Например, если у нас другая сцена с меню:
            }
        }
        catch (System.Exception ex)
        {
            ShowError("Ошибка при входе: " + ex.Message);
        }
    }

    private void ShowError(string message)
    {
        errorNotification.description = message;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
