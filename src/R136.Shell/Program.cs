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
