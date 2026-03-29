using Dapper;
using Microsoft.Data.Sqlite;
using TaskReminder.Models;

namespace TaskReminder.Data;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string dbPath = "tasks.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT,
                DueDate TEXT NOT NULL,
                RepeatType INTEGER NOT NULL DEFAULT 0,
                RepeatInterval INTEGER NOT NULL DEFAULT 0,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                LastReminderTime TEXT,
                CreatedAt TEXT NOT NULL
            )";

        await connection.ExecuteAsync(createTableSql);
    }

    public async Task<int> InsertAsync(TaskItem task)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO Tasks (Title, Description, DueDate, RepeatType, RepeatInterval, IsCompleted, LastReminderTime, CreatedAt)
            VALUES (@Title, @Description, @DueDate, @RepeatType, @RepeatInterval, @IsCompleted, @LastReminderTime, @CreatedAt);
            SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(sql, task);
    }

    public async Task UpdateAsync(TaskItem task)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            UPDATE Tasks
            SET Title = @Title,
                Description = @Description,
                DueDate = @DueDate,
                RepeatType = @RepeatType,
                RepeatInterval = @RepeatInterval,
                IsCompleted = @IsCompleted,
                LastReminderTime = @LastReminderTime,
                CreatedAt = @CreatedAt
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, task);
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "DELETE FROM Tasks WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM Tasks ORDER BY DueDate";
        return await connection.QueryAsync<TaskItem>(sql);
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM Tasks WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<TaskItem>(sql, new { Id = id });
    }
}
