using R136.Entities.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.General
{
	public enum ResultCode
	{
		Success,
		Failure,
		ContinuationRequested
	}

	public class Result
	{
		public static Result Success { get; } = new Result(ResultCode.Success);
		public static Result Failure { get; } = new Result(ResultCode.Failure);

		public ResultCode Code { get; }
		public ICollection<string>? Message { get; }
		public ContinuationStatus? ContinuationStatus { get; }

		public Result(ResultCode code)
			=> Code = code;

		public Result(ResultCode code, ICollection<string>? message)
			=> (Code, Message) = (code, message);

		public Result(ResultCode code, ICollection<string>? message, ContinuationStatus status)
			=> (Code, Message, ContinuationStatus) = (code, message, status);
	}
}
