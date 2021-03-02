
using R136.Core;
using R136.Shell.Tools;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace R136.Shell
{
  class Program
  {
    static async Task<int> Main(string[] args)
    {
      var services = Environment.Setup(args);

      return await new GameConsole(services).Play();
    }
	}
}
