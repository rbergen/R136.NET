using Microsoft.Extensions.Primitives;
using R136.Interfaces;
using System.Threading.Tasks;

namespace R136.Core
{
	public interface IEngine : IContinuable, ISnappable<Engine.Snapshot>
	{
		void StartLoadEntities(string[] groupLabels);
		Task<bool> SetEntityGroup(string label);
		Task<bool> Initialize(string? groupLabel);
		bool IsInitialized { get; }
		InputSpecs CommandInputSpecs { get; }
		StringValues StartMessage { get; }
		StringValues RoomStatus { get; }
		Result ProgressAnimateStatus();
		NextStep DoNext { get; }
		Result Run(string input);
		ICommandCallbacks? CommandCallbacks { get; set; }
	}

	public enum NextStep : byte
	{
		ShowStartMessage,
		ShowRoomStatus,
		ProgressAnimateStatus,
		RunCommand
	}
}
