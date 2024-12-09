using UnityEngine;
using System.Data;
using System.Collections;
using Npgsql;

public class DatabaseConnector : MonoBehaviour
{
    void Start()
    {
        ConnectToDatabase();
    }

    void ConnectToDatabase()
    {
        // Используем роль store_manager и его пароль
        string connectionString = "Server=172.20.7.54;Port=5432;User Id=store_manager;Password=manager_password;Database=db2093_02";
        string requestToDatabase = "SELECT name, price FROM techstore.products;";

        try
        {
            using (NpgsqlConnection connectionToDatabase = new NpgsqlConnection(connectionString))
            {
                connectionToDatabase.Open();
                Debug.Log("Подключение к базе данных успешно!");

                using (NpgsqlCommand cmd = new NpgsqlCommand(requestToDatabase, connectionToDatabase))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader.GetString(0);
                            decimal price = reader.GetDecimal(1);

                            Debug.Log($"Товар: {productName}, Цена: {price}");
                        }
                    }
                }

                connectionToDatabase.Close();
                Debug.Log("Подключение к базе данных закрыто.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Ошибка при подключении к базе данных: " + ex.Message);
        }
    }
}