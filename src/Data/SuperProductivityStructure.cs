using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Todoist2SuperProductivity.Data
{
	// Messy class to represent Super Productivity export data
	internal class SuperProductivityStructure
	{
		[JsonPropertyName("tag")]
		public Wrapper<Tag> Tags { get; set; } = new();
		[JsonPropertyName("project")]
		public Wrapper<Project> Projects { get; set; } = new();
		[JsonPropertyName("task")]
		public Wrapper<Task> Tasks { get; set; } = new();

		[JsonPropertyName("lastArchiveUpdate")]
		public long LastArchiveUpdate { get; set; }


		[JsonExtensionData]
		public Dictionary<string, object> ExtensionData { get; set; } = [];

		public class Wrapper<T>
		{
			[JsonPropertyName("ids")]
			public HashSet<string> Ids { get; set; } = [];
			[JsonPropertyName("entities")]
			public Dictionary<string, T> Entities { get; set; } = [];

			public void AddRange(IEnumerable<T> entities, Func<T, string> idSelector)
			{
				foreach (var entity in entities)
				{
					var id = idSelector(entity);

					if (!Ids.Add(id))
					{
						id = $"{id} (Todoist-{DateTimeOffset.Now.ToUnixTimeSeconds()})";
						Ids.Add(id);
					}

					Entities[id] = entity;
				}
			}
		}

		public class Tag
		{
			public required string id { get; set; }
			public required string title { get; set; }
			public required string icon { get; set; }

			public List<string> taskIds { get; set; } = [];

			public string? color { get; set; }
			public long created { get; set; }
			public Advancedcfg advancedCfg { get; set; } = new();
			public Theme theme { get; set; } = new();

			// TODO ?
			public Workstart? workStart { get; set; }
			public Workend? workEnd { get; set; }

			public Breaktime breakTime { get; set; } = new();
			public Breaknr breakNr { get; set; } = new();
		}

		public class Advancedcfg
		{
			public Worklogexportsettings worklogExportSettings { get; set; } = new();
		}

		public class Worklogexportsettings
		{
			public string[] cols { get; set; } = [
			  "DATE",
			  "START",
			  "END",
			  "TIME_CLOCK",
			  "TITLES_INCLUDING_SUB"];
			public object? roundWorkTimeTo { get; set; }
			public object? roundStartTimeTo { get; set; }
			public object? roundEndTimeTo { get; set; }
			public string separateTasksBy { get; set; } = " | ";
			public string groupBy { get; set; } = "DATE";
		}

		public class Theme
		{
			public bool isAutoContrast { get; set; } = true;
			public bool isDisableBackgroundGradient { get; set; }
			public string primary { get; set; } = "#29a1aa";
			public string huePrimary { get; set; } = "500";
			public string accent { get; set; } = "#ff4081";
			public string hueAccent { get; set; } = "500";
			public string warn { get; set; } = "#e11826";
			public string hueWarn { get; set; } = "500";
			public string? backgroundImageDark { get; set; }
			public string? backgroundImageLight { get; set; }
		}

		public class Workstart
		{
			public long _20250119 { get; set; }
			public long _20250120 { get; set; }
		}

		public class Workend
		{
			public long _20250119 { get; set; }
			public long _20250120 { get; set; }
		}

		public class Breaktime
		{
		}

		public class Breaknr
		{
		}

		public class Task
		{
			public required string id { get; set; }
			public required string projectId { get; set; }
			public required string title { get; set; }

			public object[] subTaskIds { get; set; } = [];

			// has to be empty object, not null
			//public Timespentonday? timeSpentOnDay { get; set; }
			public object timeSpentOnDay { get; set; } = new object();

			public int timeSpent { get; set; }
			public int timeEstimate { get; set; }

			public bool isDone { get; set; }
			public long? doneOn { get; set; }

			public string notes { get; set; } = "";
			public List<string> tagIds { get; set; } = [];
			public string? parentId { get; set; }
			public string? reminderId { get; set; }
			public long created { get; set; }
			public string? repeatCfgId { get; set; }
			public long? plannedAt { get; set; }
			public int _showSubTasksMode { get; set; }
			// TODO ?
			//   {
			//  "type": "LINK",
			//  "title": "fdfsdf",
			//  "path": "https://github.com/johannesjo/super-productivity/issues/2239",
			//  "id": "Tzocg9rnHDkLC6qzPJ4TW"
			//  }
			public object[] attachments { get; set; } = [];

			// Not relevant
			public object? issueId { get; set; }
			public object? issueProviderId { get; set; }
			public object? issuePoints { get; set; }
			public object? issueType { get; set; }
			public object? issueAttachmentNr { get; set; }
			public object? issueLastUpdated { get; set; }
			public object? issueWasUpdated { get; set; }
			public object? issueTimeTracked { get; set; }
		}

		public class Timespentonday
		{
			public int _20250119 { get; set; }
		}

		public class Project
		{
			public bool isHiddenFromMenu { get; set; }
			public bool isArchived { get; set; }
			public bool isEnableBacklog { get; set; }
			public List<string> backlogTaskIds { get; set; } = [];
			public object[] noteIds { get; set; } = [];
			// Worklog...
			public Advancedcfg advancedCfg { get; set; } = new();
			public Theme theme { get; set; } = new();

			public Workstart? workStart { get; set; }
			public Workend? workEnd { get; set; }

			public Breaktime breakTime { get; set; } = new();
			public Breaknr breakNr { get; set; } = new();

			public List<string> taskIds { get; set; } = [];
			public required string icon { get; set; }
			public required string id { get; set; }
			public required string title { get; set; }
		}
	}
}
