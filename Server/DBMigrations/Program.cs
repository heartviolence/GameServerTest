
using DbUp;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection;

var connectionString =
    args.FirstOrDefault()
?? "Data Source=localhost,1433;User ID=SA;Database=TestDB;Password=Passw0rd;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";
//?? "Server=(localdb)\\mssqllocaldb;Database=TestDatabase;Trusted_Connection=True;";

EnsureDatabase.For.SqlDatabase(connectionString);

var upgrader =
    DeployChanges.To
        .SqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .LogToConsole()
        .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
#if DEBUG
    Console.ReadLine();
#endif
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Success!");
Console.ResetColor();
return 0;