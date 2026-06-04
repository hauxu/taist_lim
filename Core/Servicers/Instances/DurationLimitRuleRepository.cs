using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Core.Models.DurationLimit;
using Core.Librarys;
using Core.Servicers.Interfaces;
using System.Linq;

namespace Core.Servicers.Instances
{
    public class DurationLimitRuleRepository : IDurationLimitRuleRepository, IDisposable
    {
        private readonly string _connectionString;
        private readonly object _dbLock = new object();

        public DurationLimitRuleRepository()
        {
            _connectionString = GetConnectionString();
            EnsureDatabaseDirectory();
            InitializeTable();
        }

        private static string GetConnectionString()
        {
            string dbFile = Path.Combine(FileHelper.GetRootDirectory(), "Data", "data.db");
            return $"Data Source={dbFile};Version=3;";
        }

        private void EnsureDatabaseDirectory()
        {
            string dbFile = Path.Combine(FileHelper.GetRootDirectory(), "Data", "data.db");
            string dir = Path.GetDirectoryName(dbFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        public void InitializeTable()
        {
            lock (_dbLock)
            {
                string sql = @"
                    CREATE TABLE IF NOT EXISTS DurationLimitRules (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        RuleType INTEGER NOT NULL,
                        TargetName TEXT NOT NULL,
                        TargetProcessName TEXT,
                        MaxDailyMinutes INTEGER NOT NULL,
                        LockAction INTEGER NOT NULL,
                        IsEnabled INTEGER DEFAULT 1,
                        CreateTime TEXT NOT NULL,
                        UpdateTime TEXT NOT NULL
                    );";
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                        cmd.ExecuteNonQuery();
                }
            }
        }

        public List<DurationLimitRule> GetAllRules()
        {
            lock (_dbLock)
            {
                var rules = new List<DurationLimitRule>();
                string sql = "SELECT * FROM DurationLimitRules ORDER BY RuleType, TargetName";
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            rules.Add(MapReaderToRule(reader));
                }
                return rules;
            }
        }

        public List<DurationLimitRule> GetAllEnabledRules()
            => GetAllRules().Where(r => r.IsEnabled).ToList();

        public void AddRule(DurationLimitRule rule)
        {
            rule.CreateTime = DateTime.Now;
            rule.UpdateTime = DateTime.Now;
            lock (_dbLock)
            {
                string sql = @"INSERT INTO DurationLimitRules
                    (RuleType, TargetName, TargetProcessName, MaxDailyMinutes, LockAction, IsEnabled, CreateTime, UpdateTime)
                    VALUES (@RuleType, @TargetName, @TargetProcessName, @MaxDailyMinutes, @LockAction, @IsEnabled, @CreateTime, @UpdateTime);";
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        AddParameters(cmd, rule);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void UpdateRule(DurationLimitRule rule)
        {
            rule.UpdateTime = DateTime.Now;
            lock (_dbLock)
            {
                string sql = @"UPDATE DurationLimitRules SET
                    RuleType=@RuleType, TargetName=@TargetName, TargetProcessName=@TargetProcessName,
                    MaxDailyMinutes=@MaxDailyMinutes, LockAction=@LockAction, IsEnabled=@IsEnabled,
                    UpdateTime=@UpdateTime WHERE Id=@Id";
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        AddParameters(cmd, rule);
                        cmd.Parameters.AddWithValue("@Id", rule.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeleteRule(int ruleId)
        {
            lock (_dbLock)
            {
                string sql = "DELETE FROM DurationLimitRules WHERE Id=@Id";
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", ruleId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private DurationLimitRule MapReaderToRule(SQLiteDataReader reader) => new DurationLimitRule
        {
            Id = Convert.ToInt32(reader["Id"]),
            RuleType = (RuleType)Convert.ToInt32(reader["RuleType"]),
            TargetName = reader["TargetName"].ToString(),
            TargetProcessName = reader["TargetProcessName"] == DBNull.Value ? null : reader["TargetProcessName"].ToString(),
            MaxDailyMinutes = Convert.ToInt32(reader["MaxDailyMinutes"]),
            LockAction = (LockAction)Convert.ToInt32(reader["LockAction"]),
            IsEnabled = Convert.ToInt32(reader["IsEnabled"]) == 1,
            CreateTime = DateTime.Parse(reader["CreateTime"].ToString()),
            UpdateTime = DateTime.Parse(reader["UpdateTime"].ToString())
        };

        private void AddParameters(SQLiteCommand cmd, DurationLimitRule rule)
        {
            cmd.Parameters.AddWithValue("@RuleType", (int)rule.RuleType);
            cmd.Parameters.AddWithValue("@TargetName", rule.TargetName);
            cmd.Parameters.AddWithValue("@TargetProcessName", string.IsNullOrEmpty(rule.TargetProcessName) ? (object)DBNull.Value : rule.TargetProcessName);
            cmd.Parameters.AddWithValue("@MaxDailyMinutes", rule.MaxDailyMinutes);
            cmd.Parameters.AddWithValue("@LockAction", (int)rule.LockAction);
            cmd.Parameters.AddWithValue("@IsEnabled", rule.IsEnabled ? 1 : 0);
            cmd.Parameters.AddWithValue("@CreateTime", rule.CreateTime.ToString("o"));
            cmd.Parameters.AddWithValue("@UpdateTime", rule.UpdateTime.ToString("o"));
        }

        public void Dispose() { }
    }
}
