using System.Collections.Generic;

namespace R136.BuildTool.Tasks
{
	class ConversionTask
	{
		public string Directory { get; set; } = string.Empty;
		public Dictionary<Entity, FileNames?>? Conversions { get; set; }
	}
}
