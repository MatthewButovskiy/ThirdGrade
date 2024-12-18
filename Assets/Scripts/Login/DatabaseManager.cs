using UnityEngine;
using Npgsql;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;

    private NpgsqlConnection connection;
    private string currentRole; 
    private string connectionString;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetConnectionString(string connStr)
    {
        connectionString = connStr;
    }

    public bool OpenConnection()
    {
        try
        {
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
            Debug.Log("Соединение открыто");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Не удалось открыть соединение: " + ex.Message);
            return false;
        }
    }

    public void CloseConnection()
    {
        if (connection != null)
        {
            connection.Close();
            connection = null;
            Debug.Log("Соединение закрыто");
        }
    }

    public void SetRole(string role)
    {
        currentRole = role;
    }

    public string GetRole()
    {
        return currentRole;
    }

    public NpgsqlConnection GetConnection()
    {
        return connection;
    }
}