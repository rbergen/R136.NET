using Microsoft.Extensions.Primitives;
using R136.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Core
{
	public interface IEngine : IContinuable, ISnappable<Engine.Snapshot>
	{
		void StartLoadEntities(string[] groupLabels);
		bool SetEntityGroup(string label);
		bool Initialize();
		bool IsInitialized { get; }
		InputSpecs CommandInputSpecs { get; }
		StringValues StartMessage { get; }
		StringValues RoomStatus { get; }
		StringValues ProgressAnimateStatus();
		NextStep DoNext { get; }
		Result Run(string input);
		void EndPause();
	}

	public enum NextStep
	{
		ShowStartMessage,
		ShowRoomStatus,
		ProgressAnimateStatus,
		RunCommand,
		Pause
	}
}
