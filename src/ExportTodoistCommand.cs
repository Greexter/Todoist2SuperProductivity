using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Net.Http.Headers;

internal class ExportTodoistCommand : AsyncCommand<ExportTodoistSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ExportTodoistSettings settings)
	{
		using var fs = File.OpenWrite(settings.ExportOutputFile);

		var token = AnsiConsole.Prompt(new TextPrompt<string>("Please enter your Todoist API token:\n")
		{
			IsSecret = true,
			AllowEmpty = false
		});

		using var client = new HttpClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		using var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{"sync_token", "*"},
			{"resource_types", "[\"all\"]" }
		});

		AnsiConsole.WriteLine("Fetching data...");
		var response = await client.PostAsync("https://api.todoist.com/sync/v9/sync", requestContent);
		if (!response.IsSuccessStatusCode)
		{
			AnsiConsole.WriteLine($"Request Error: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
			return 1;
		}

		AnsiConsole.WriteLine($"Saving data to {settings.ExportOutputFile}");
		using var stream = (await response.Content.ReadAsStreamAsync());
		await stream.CopyToAsync(fs);
		return 0;
	}
}

internal class ExportTodoistSettings : CommandSettings
{
	[CommandOption("-o|--output")]
	[Description("Where to save the exported data from Todoist.")]
	public string ExportOutputFile { get; internal set; } = "./todoist_sync_export.json";
}