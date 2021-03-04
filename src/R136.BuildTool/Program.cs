using R136.BuildTool.Tools;
using R136.Entities;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace R136.BuildTool
{
	partial class Program
	{
		private static readonly JsonSerializerOptions _entityDeserializerOptions = new() 
		{ 
			ReadCommentHandling = JsonCommentHandling.Skip, 
			AllowTrailingCommas = true 
		};

		static void Main(string[] args)
		{
			var arguments = Arguments.Parse(args);

			if (arguments.ConfigFileName != null)
				RunAutomatic(arguments);
			
			else
				RunManual();

		}

	}
}
