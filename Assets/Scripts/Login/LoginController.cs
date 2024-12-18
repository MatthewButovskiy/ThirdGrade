using Michsky.MUIP;
using UnityEngine;
using Npgsql;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

public class LoginController : MonoBehaviour
{
    [SerializeField] private CustomInputField loginField;
    [SerializeField] private CustomInputField passwordField;
    [SerializeField] private NotificationManager errorNotification; 
    [SerializeField] private MainMenuController startArea;
    [SerializeField] private ButtonManager loginButton;       // Ссылка на кнопку "Войти"
    [SerializeField] private GameObject loaderObject;  // Ссылка на объект лоадера

    private string server = "172.20.7.54";
    private string port = "5432";
    private string database = "db2093_02";

    public async void OnLoginClick()
    {
        string login = loginField.inputText.text.Trim();
        string password = passwordField.inputText.text.Trim();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            ShowError("Пожалуйста, введите логин и пароль.");
            return;
        }

        string connectionString = $"Server={server};Port={port};User Id={login};Password={password};Database={database}";

        loginButton.isInteractable = false;
        loaderObject.SetActive(true);

        bool success = false;
        string errorMessage = "";

        // Асинхронно подключаемся в Task.Run()
        await Task.Run(() =>
        {
            try
            {
                using (var testConn = new NpgsqlConnection(connectionString))
                {
                    testConn.Open(); // Если зависает, то хотя бы UI продолжит работать, потому что в другом потоке
                    testConn.Close();
                }

                DatabaseManager.Instance.SetConnectionString(connectionString);
                success = DatabaseManager.Instance.OpenConnection(); // Open тоже можно сделать в Task.Run если нужно
                if (!success) errorMessage = "Не удалось открыть постоянное соединение.";
                else DatabaseManager.Instance.SetRole(login);
            }
            catch (System.Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
            }
        });

        loaderObject.SetActive(false);
        loginButton.isInteractable = true;

        if (success)
        {
            gameObject.SetActive(false);
            startArea.gameObject.SetActive(true);
            startArea.LogIn();
        }
        else
        {
            ShowError("Ошибка при входе: " + errorMessage);
        }
    }

    private IEnumerator TryConnectToDatabase(string connectionString, string login)
    {
        yield return null; // На случай если надо чуть подождать

        NpgsqlConnection testConn = new NpgsqlConnection(connectionString);
        bool success = false;
        string errorMessage = "";

        // Пытаемся подключиться в try-catch
        try
        {
            testConn.Open();
            Debug.Log("Подключение к базе данных успешно!");
            Debug.Log($"Logged in as {login}");
            testConn.Close();

            // Если получилось открыть и закрыть testConn - креды верны
            DatabaseManager.Instance.SetConnectionString(connectionString);
            if (DatabaseManager.Instance.OpenConnection())
            {
                DatabaseManager.Instance.SetRole(login);
                success = true;
            }
        }
        catch (System.Exception ex)
        {
            errorMessage = ex.Message;
            success = false;
        }

        // Отключаем лоадер
        loaderObject.SetActive(false);
        // Возвращаем кнопку в активный статус
        loginButton.isInteractable = true;

        if (success)
        {
            // Всё ок, скрываем текущий объект (панель логина)
            gameObject.SetActive(false);
            // Показываем стартовую зону
            startArea.gameObject.SetActive(true);
            startArea.LogIn();
        }
        else
        {
            // Показываем ошибку
            if (string.IsNullOrEmpty(errorMessage))
                errorMessage = "Не удалось подключиться. Проверьте логин и пароль.";

            ShowError("Ошибка при входе: " + errorMessage);
        }
    }

    private void ShowError(string message)
    {
        errorNotification.description = message;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
