using System;
using Microsoft.Data.SqlClient;

namespace Datahub.Tests.Migrations;

/// <summary>
/// Helper for creating and destroying unique SQL Server LocalDB databases in tests.
/// </summary>
public static class LocalDbUtils
{
 public static string CreateUniqueLocalDbDatabase(string prefix)
 {
 var dbName = $"{prefix}_{Guid.NewGuid():N}";
 using var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true");
 conn.Open();
 using var cmd = conn.CreateCommand();
 cmd.CommandText = $"CREATE DATABASE [{dbName}]";
 cmd.ExecuteNonQuery();
 return dbName;
 }

 public static void DropLocalDbDatabase(string dbName)
 {
 try
 {
 using var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true");
 conn.Open();
 using var cmd = conn.CreateCommand();
 cmd.CommandText = $@"IF DB_ID('{dbName}') IS NOT NULL BEGIN ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}]; END";
 cmd.ExecuteNonQuery();
 }
 catch
 {
 // best-effort cleanup
 }
 }
}
