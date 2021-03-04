using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.BuildTool.Tasks
{
	class ConversionTask
	{
		public string Directory { get; set; } = string.Empty;
		public Dictionary<Entity, FileNames?>? Conversions { get; set; }
	}
}
