using System.Collections.Generic;

namespace R136.Interfaces
{
	public interface IEngine : IContinuable
	{
		InputSpecs CommandInputSpecs { get; }
		public ICollection<string>? RoomStatus { get; }
		public NextStep DoNext { get; }
		Result Run(string input);
	}

	public enum NextStep
	{
		ShowRoomStatus,
		ProgressAnimateStatus,
		RunCommand
	}
}
