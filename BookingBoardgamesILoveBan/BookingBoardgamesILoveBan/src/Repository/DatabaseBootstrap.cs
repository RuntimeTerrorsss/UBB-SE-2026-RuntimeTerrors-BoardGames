using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Windows.Media.Streaming.Adaptive;

public static class DatabaseBootstrap
{
    private static string masterConnection;
    private static string appConnection;
    private static string databaseName;

    public static string GetAppConnection() => appConnection;

    public static string GetProjectRoot()
    {
        // FIXME - REGEX-like implementation that is slightly nicer but still iffy
        string currentPath = AppContext.BaseDirectory;
        int binIndex = currentPath.IndexOf("\\bin\\", StringComparison.OrdinalIgnoreCase);
        if (binIndex != -1)
        {
            return currentPath.Substring(0, binIndex);
        }
        return currentPath;
    }
    private static void LoadConfig()
    {
        string root = GetProjectRoot();
        string path = Path.Combine(root, "appsettings.json");

        if (!File.Exists(path))
        {
            throw new Exception("appsettings.json file not found! Copy appsettings.example.json, rename it, and replace placeholders! Do NOT just rename the file, make sure to COPY it first <3");
        }
        string jsonContent = File.ReadAllText(path);
        using var document = JsonDocument.Parse(jsonContent);
        var rootElement = document.RootElement;

        masterConnection = rootElement.GetProperty("MasterConnection").GetString();
        appConnection = rootElement.GetProperty("AppConnection").GetString();
        databaseName = rootElement.GetProperty("DatabaseName").GetString();
    }
    public static string GetSchemaSql()
    {
        string sqlFilePath = Path.Combine(GetProjectRoot(), "DatabaseSchema.sql");
        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException($"Could not find DatabaseSchema.sql in the app folder. Path: {sqlFilePath}");
        }
        return File.ReadAllText(sqlFilePath);
    }
    public static string GetMockData()
    {
        string mockFilePath = Path.Combine(GetProjectRoot(), "MockData.sql");
        if (!File.Exists(mockFilePath))
        {
            throw new FileNotFoundException($"Could not find MockData.sql in the app folder. Path: {mockFilePath}");
        }
        return File.ReadAllText(mockFilePath);
    }

    public static void Initialize()
    {
        LoadConfig();
        try
        {
            // Create Rental App Database if it does not exist
            using (var connection = new SqlConnection(masterConnection))
            {
                connection.Open();
                var command = new SqlCommand($@"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
                    CREATE DATABASE [{databaseName}];
                ", connection);
                command.ExecuteNonQuery();
                connection.Close();
            }

            // Load database tables from SQL script (schema only)
            // and Add System as User 0
            string schemaSql = GetSchemaSql();
            using (var connection = new SqlConnection(appConnection))
            {
                connection.Open();
                var schemaCommand = new SqlCommand(schemaSql, connection);
                schemaCommand.ExecuteNonQuery();
                connection.Close();
                System.Diagnostics.Debug.WriteLine("Database Schema applied successfully.");
            }

            // add mock data
            string mockData = GetMockData();
            using (var connection = new SqlConnection(appConnection))
            {
                connection.Open();
                var mockDataCommand = new SqlCommand(mockData, connection);
                mockDataCommand.ExecuteNonQuery();
                connection.Close();
                System.Diagnostics.Debug.WriteLine("Mock Data added successfully.");
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {exception.Message}");
        }
    }
}