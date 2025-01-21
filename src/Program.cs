using Microsoft.Extensions.Hosting;
using Todoist2SuperProductivity;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Reflection;

var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
AnsiConsole.WriteLine($"Todoist2SuperProductivity {version}");
AnsiConsole.WriteLine();

var builder = Host.CreateDefaultBuilder(args);

var app = new CommandApp();

app.Configure(config =>
{
	config.AddCommand<ExportTodoistCommand>("export-todoist")
		.WithAlias("export");
	config.AddCommand<ConvertCommand>("convert");
});

//return app.Run("convert".Split());
//return app.Run("export".Split());
return app.Run(args);