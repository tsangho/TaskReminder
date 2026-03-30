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

        // 创建任务表（包含所有必要字段）
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
                CompletedTime TEXT,
                CreatedAt TEXT NOT NULL
            )";

        await connection.ExecuteAsync(createTableSql);
        
        // 检查是否需要添加新字段（兼容性处理）
        await AddMissingColumnsAsync(connection);
    }

    /// <summary>
    /// 添加可能缺失的列（用于旧版本数据库升级）
    /// </summary>
    private async Task AddMissingColumnsAsync(SqliteConnection connection)
    {
        try
        {
            // 检查 CompletedTime 列是否存在
            var checkSql = "PRAGMA table_info(Tasks)";
            var columns = await connection.QueryAsync<string>(checkSql);
            var columnList = columns.ToList();

            if (!columnList.Contains("CompletedTime"))
            {
                await connection.ExecuteAsync("ALTER TABLE Tasks ADD COLUMN CompletedTime TEXT");
                Console.WriteLine("[DatabaseService] 已添加 CompletedTime 列");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseService] 检查/添加列失败: {ex.Message}");
        }
    }

    public async Task<int> InsertAsync(TaskItem task)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO Tasks (Title, Description, DueDate, RepeatType, RepeatInterval, IsCompleted, LastReminderTime, CompletedTime, CreatedAt)
            VALUES (@Title, @Description, @DueDate, @RepeatType, @RepeatInterval, @IsCompleted, @LastReminderTime, @CompletedTime, @CreatedAt);
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
                CompletedTime = @CompletedTime,
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

    /// <summary>
    /// 获取所有待处理且已到期的任务
    /// </summary>
    public async Task<IEnumerable<TaskItem>> GetPendingOverdueTasksAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var sql = @"SELECT * FROM Tasks 
                    WHERE IsCompleted = 0 AND DueDate <= @Now 
                    ORDER BY DueDate";
        return await connection.QueryAsync<TaskItem>(sql, new { Now = now });
    }
}
