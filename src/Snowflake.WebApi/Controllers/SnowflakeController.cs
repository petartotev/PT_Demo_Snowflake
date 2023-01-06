using Microsoft.AspNetCore.Mvc;
using Snowflake.Data.Client; // 2.0.10
using System.Data;
using System.Data.Odbc; // 7.0.0
using System.Text;

namespace Snowflake.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SnowflakeController : ControllerBase
    {
        // 1. Install NuGet package Snowflake.Data (2.0.10). (2.0.19) was tried, but it threw an exception!!!
        // 2. Implement code below.
        // 3. When you run the application and hit the endpoint, the browser will open an authentication window.
        [HttpGet("dotnetdriver")]
        public IActionResult GetWithDotnetDriver()
        {
            var result = string.Empty;

            using (IDbConnection conn = new SnowflakeDbConnection())
            {
                conn.ConnectionString = "account=<account>;user=<user>;authenticator=externalbrowser;";
                
                conn.Open();

                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "show databases;"; // Set up query

                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = reader.GetString(0); // Display query result in the console
                }

                conn.Close();
            }

            return Ok(result);
        }

        // 1. Install both snowflake32_odbc-2.25.4.msi and snowflake64_odbc-2.25.7.msi.
        // 2. Open ODBC Data Sources (32-bit) > System DSN > Add... > Create SnowflakeDSIIDriver > Snowflake Configuration Dialog:
        // Data Source = <datasourcename>
        // User = <user>
        // Server = <server>.snowflakecomputing.com
        // Authenticator = externalbrowser
        // 3. Just in case, open ODBC Data Sources (64-bit) and create an identical driver with the same properties.
        // 4. Install NuGet package System.Data.Odbc 7.0.0.
        // 5. Implement code below.
        // 6. When you run the application and hit the endpoint, the browser will open an authentication window.
        [HttpGet("odbcdriver")]
        public IActionResult GetWithOdbcDriver()
        {
            var connectionString = "DSN=<datasourcename>;Server=<server>.snowflakecomputing.com;uid=<user>";

            var stringBuilder = new StringBuilder();

            using (OdbcConnection conn = new(connectionString))
            {
                conn.Open();

                Console.WriteLine("Connected. Server version: " + conn.ServerVersion);

                string cmdPut = "show databases;";

                OdbcCommand cmd = new(cmdPut, conn);

                Console.WriteLine("Running query: " + cmdPut);

                var reader = cmd.ExecuteReader();

                var columns = new List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                stringBuilder.AppendLine(string.Join(",", columns));

                while (reader.Read())
                {
                    for (int i = 0; i < columns.Count; i++)
                    {
                        stringBuilder.Append(!string.IsNullOrWhiteSpace(reader[i].ToString()) ? reader[i].ToString() : "NULL");
                        stringBuilder.Append(i < columns.Count - 1 ? "," : string.Empty);
                    }

                    stringBuilder.AppendLine();
                }

                reader.Close();

                conn.Close();
            }

            return Ok(stringBuilder.ToString());
        }
    }
}