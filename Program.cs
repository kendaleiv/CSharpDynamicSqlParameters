using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpDynamicSqlParameters
{
    class Program
    {
        const string Filename = "dynamic-parameters.db";

        static async Task Main(string[] args)
        {
            Cleanup();
            await Setup();
            await DynamicParametersQuery();
        }

        static async Task DynamicParametersQuery()
        {
            var items = new[] { "a", "b", "c" };
            var connection = new SqliteConnection($"Data Source={Filename}");
            await connection.OpenAsync();

            var parameters = items.Select((x, i) => Tuple.Create($"@{i}", x));

            var command = connection.CreateCommand();
            command.CommandText = $"select count(*) from TestTable where name in ({string.Join(",", parameters.Select(x => x.Item1))})";

            Console.WriteLine(command.CommandText);

            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
            }

            var count = (long)await command.ExecuteScalarAsync();

            Console.WriteLine($"Count: {count}");
        }

        static void Cleanup()
        {
            if (File.Exists(Filename))
            {
                File.Delete(Filename);
            }
        }

        static async Task Setup()
        {
            using var connection = new SqliteConnection($"Data Source={Filename}");
            await connection.OpenAsync();

            var createCommand = connection.CreateCommand();
            createCommand.CommandText = "create table TestTable (name nvarchar(255)); insert into TestTable values ('a'), ('b'), ('c'), ('d');";

            await createCommand.ExecuteNonQueryAsync();
        }
    }
}
