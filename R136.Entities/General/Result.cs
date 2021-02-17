using System;
using System.Collections.Generic;

namespace R136.Entities.General
{
	public enum ResultCode
	{
		Success,
		Failure,
		ContinuationRequested,
		EndRequested
	}

	public class Result
	{
		private static readonly Result _success = new Result(ResultCode.Success);
		private static readonly Result _failure = new Result(ResultCode.Failure);

		public static Result Success() => _success;
		public static Result Success(ICollection<string>? message) => new Result(ResultCode.Success, message);
		public static Result Failure() => _failure;
		public static Result Failure(ICollection<string>? message) => new Result(ResultCode.Failure, message);
		public static Result EndRequested() => new Result(ResultCode.EndRequested);
		public static Result ContinuationRequested(ContinuationStatus status, InputSpecs specs, ICollection<string>? message)
			=> new Result(status, specs, message);

		public ResultCode Code { get; }
		public ICollection<string>? Message { get; }
		public ContinuationStatus? ContinuationStatus { get; }

		public InputSpecs? InputSpecs { get; }
		public bool IsSuccess => Code == ResultCode.Success;
		public bool IsFailure => Code == ResultCode.Failure;
		public bool IsContinuationRequest => Code == ResultCode.ContinuationRequested;
		public bool IsEndRequest => Code == ResultCode.EndRequested;

		public Result(ResultCode code)
			=> Code = code;

		public Result(ResultCode code, ICollection<string>? message)
		{
			if (code == ResultCode.ContinuationRequested)
				throw new ArgumentException("ResultCode ContinuationRequested requires InputSpecs", nameof(code));

			(Code, Message) = (code, message);
		}

		public Result(ContinuationStatus status, InputSpecs specs, ICollection<string>? message)
			=> (Code, ContinuationStatus, InputSpecs, Message)
			= (ResultCode.ContinuationRequested, status, specs, message);
	}
}
