using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Interfaces
{
	public interface IEngine : IContinuable
	{
		Task<bool> Initialize();
		InputSpecs CommandInputSpecs { get; }
		ICollection<string>? StartMessage { get; }
		ICollection<string>? RoomStatus { get; }
		ICollection<string>? ProgressAnimateStatus();
		NextStep DoNext { get; }
		Result Run(string input);
	}

	public enum NextStep
	{
		ShowStartMessage,
		ShowRoomStatus,
		ProgressAnimateStatus,
		RunCommand
	}
}
