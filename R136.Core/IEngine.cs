using R136.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Core
{
	public interface IEngine : IContinuable, ISnappable<Engine.Snapshot>
	{
		void StartLoadEntities(string[] groupLabels);
		Task<bool> SetEntityGroup(string label);
		Task<bool> Initialize(string groupLabel);
		InputSpecs CommandInputSpecs { get; }
		ICollection<string>? StartMessage { get; }
		ICollection<string>? RoomStatus { get; }
		ICollection<string>? ProgressAnimateStatus();
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
