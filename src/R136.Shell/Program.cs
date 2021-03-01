
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
    static int Main(string[] args)
    {
      return new GameConsole(Environment.Setup(args)).Play();
    }
	}
}
