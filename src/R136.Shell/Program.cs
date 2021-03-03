using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R136.Shell.Tools;
using System.Threading.Tasks;

namespace R136.Shell
{
	class Program
  {
    static async Task<int> Main(string[] args)
      => await new GameConsole(Environment.Setup(args)).Play();
	}
}
