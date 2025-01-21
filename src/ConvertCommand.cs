using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;
using Todoist2SuperProductivity.Data;

namespace Todoist2SuperProductivity
{
	internal class ConvertCommand : AsyncCommand<ConvertSettings>
	{
		public override async Task<int> ExecuteAsync(CommandContext context, ConvertSettings settings)
		{
			if (!File.Exists(settings.TodoistExportFile))
			{
				AnsiConsole.WriteLine($"Please provide an existing path to the Todoist export. Nothing at '{settings.TodoistExportFile}'.");
				return 1;
			}

			if (!File.Exists(settings.SuperProductivityExportFile))
			{
				AnsiConsole.WriteLine($"Please provide an existing path to the Super Productivity export. Nothing at '{settings.SuperProductivityExportFile}'");
				return 1;
			}

			using var todoistFs = File.OpenRead(settings.TodoistExportFile);
			using var spFs = File.OpenRead(settings.SuperProductivityExportFile);

			AnsiConsole.WriteLine("Parsing data...");
			var syncData = await JsonSerializer.DeserializeAsync<SyncData>(todoistFs) ?? throw new InvalidOperationException("Failed to parse the sync data. Did the Todoist API change?");
			var superProductivity = await JsonSerializer.DeserializeAsync<SuperProductivityStructure>(spFs) ?? throw new InvalidOperationException("Failed to parse the sync data. Did the Todoist API change?");

			AnsiConsole.WriteLine($"Todoist stats\nProjects: {syncData.Projects.Count}, Tasks: {syncData.Tasks.Count}, Labels: {syncData.Labels.Count}");

			AnsiConsole.WriteLine("Converting data...");
			var newProjects = syncData.Projects.Select(x => new SuperProductivityStructure.Project
			{
				id = x.id,
				title = x.name,
				isArchived = x.is_archived,
				icon = settings.DefaultProjectIcon,
				theme = new SuperProductivityStructure.Theme { primary = x.color },
			}).ToList();

			var newTags = syncData.Labels.Select(x => new SuperProductivityStructure.Tag
			{
				id = x.id,
				title = x.name,
				icon = settings.DefaultTagIcon,
				// assign only primary, should be used for both
				color = null,
				theme = new SuperProductivityStructure.Theme { primary = x.color },
			}).ToList();

			var tagByNameLookup = newTags.ToDictionary(x => x.title);
			var newTasks = syncData.Tasks.Select(x => new SuperProductivityStructure.Task
			{
				id = x.id,
				created = new DateTimeOffset(x.added_at).ToUnixTimeMilliseconds(),
				projectId = x.project_id,
				title = x.content,
				// do not have completed exported for now (could be done with
				// https://developer.todoist.com/sync/v9/#get-all-completed-items)
				//doneOn = x.completed_at,
				isDone = false,

				// TODO ?
				//timeEstimate = x.duration 
				timeSpent = 0,

				// TODO
				repeatCfgId = null,

				tagIds = x.labels.Select(x => tagByNameLookup[x].id).ToList(),

				// not relevant
				issueAttachmentNr = 0,
				issueId = null,
				issueLastUpdated = null,
				issuePoints = null,
				issueProviderId = null,
				issueTimeTracked = null,
				issueType = null,
				issueWasUpdated = null
			}).ToList();

			// Add tasks to their projects
			var projectLookup = newProjects.ToDictionary(x => x.id);
			foreach (var task in newTasks)
			{
				if (projectLookup.TryGetValue(task.projectId, out var newProject))
				{
					if (settings.AddTasksToBacklog)
					{
						newProject.isEnableBacklog = true;
						newProject.backlogTaskIds.Add(task.id);
					}
					else
					{
						newProject.taskIds.Add(task.id);
					}
				}
				else
				{
					AnsiConsole.WriteLine($"[W] Failed to lookup project {task.projectId} of task {task.id}.");
				}
			}

			// Add tasks to all their tags
			var tagLookup = newTags.ToDictionary(x => x.id);
			foreach (var task in newTasks)
			{
				foreach (var tagId in task.tagIds)
				{
					if (tagLookup.TryGetValue(tagId, out var tag))
					{
						tag.taskIds.Add(task.id);
					}
					else
					{
						AnsiConsole.WriteLine($"[W] Failed to assing tag {tagId} of task {task.id}");
					}
				}
			}

			superProductivity.Projects.AddRange(newProjects, x => x.id);
			superProductivity.Tasks.AddRange(newTasks, x => x.id);
			superProductivity.LastArchiveUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			superProductivity.Tags.AddRange(newTags, x => x.id);

			//foreach (var project in syncData.Projects)
			//{
			//	project.
			//}

			var outFile = settings.OutputFile;
			if (outFile == null)
			{
				outFile = $"./{Path.GetFileNameWithoutExtension(settings.SuperProductivityExportFile)}_out{Path.GetExtension(settings.SuperProductivityExportFile)}";
			}

			// check existing
			if (Path.Exists(outFile))
			{
				bool confirmation = AnsiConsole.Prompt(
					new ConfirmationPrompt($"File '{outFile}' already exists, do you want to overwrite it?")
					{
						DefaultValue = false
					});

				if (!confirmation)
				{
					AnsiConsole.WriteLine("Canceled");
					return 1;
				}
			}

			AnsiConsole.WriteLine($"Writing output to '{outFile}'");

			File.WriteAllText(outFile, JsonSerializer.Serialize(superProductivity));

			return 0;
		}
	}

	internal class ConvertSettings : CommandSettings
	{
		public Uri TodoistUrl { get; } = new Uri("https://api.todoist.com/sync/v9/sync");

		[CommandOption("--project-icon")]
		[Description("Icon to set for the projects. Specify one of the available options in Super Productivity.")]
		public string DefaultProjectIcon { get; set; } = "question_mark";

		[CommandOption("--tag-icon")]
		[Description("Icon to set for the tags. Specify one of the available options in Super Productivity.")]
		public string DefaultTagIcon { get; set; } = "tag";

		[CommandOption("-b|--use-backlog")]
		[Description("When present, all tasks will be placed into the backlog of their project.")]
		public bool AddTasksToBacklog { get; set; }

		[CommandOption("--todoist-export")]
		[Description("Path to the Todoist export file.")]
		public string TodoistExportFile { get; set; } = "./todoist_sync_export.json";
		[CommandOption("--sp-export")]
		[Description("Path to the Super Productivity export file.")]
		public string SuperProductivityExportFile { get; set; } = "./super-productivity-backup.json";
		[CommandOption("-o|--output")]
		[Description("Where to save the converted data.")]
		public string? OutputFile { get; set; }
	}
}
