using System;
using System.Collections.Generic;

namespace R136.Interfaces
{
	public enum ResultCode
	{
		Success,
		Failure,
		Error,
		ContinuationRequested,
		EndRequested
	}

	public class Result
	{
		private static readonly Result _success = new Result(ResultCode.Success);
		private static readonly Result _failure = new Result(ResultCode.Failure);
		private static readonly Result _error = new Result(ResultCode.Error);

		public static Result Success() => _success;
		public static Result Success(ICollection<string>? message) => new Result(ResultCode.Success, message);
		public static Result Success(string message) => new Result(ResultCode.Success, new string[] { message });

		public static Result Failure() => _failure;
		public static Result Failure(ICollection<string>? message) => new Result(ResultCode.Failure, message);
		public static Result Failure(string message) => new Result(ResultCode.Failure, new string[] { message });

		public static Result Error() => _error;
		public static Result Error(ICollection<string>? message) => new Result(ResultCode.Error, message);
		public static Result Error(string message) => new Result(ResultCode.Error, new string[] { message });

		public static Result EndRequested() => new Result(ResultCode.EndRequested);
		public static Result ContinuationRequested(ContinuationStatus status, InputSpecs specs, ICollection<string>? message)
			=> new Result(status, specs, message);

		public Result WrapContinuationRequest(IContinuable wrappingObject)
			=> IsContinuationRequest
			? ContinuationRequested(new ContinuationStatus(wrappingObject, (wrappingObject, ContinuationStatus)), InputSpecs!, Message)
			: this;

		public static ContinuationStatus? UnwrapContinuationData(IContinuable wrappingObject, object data)
			=> data is ValueTuple<IContinuable, ContinuationStatus> wrapped && wrapped.Item1 == wrappingObject
			? wrapped.Item2
			: null;

		public static Result ContinueWrappedContinuationData(IContinuable wrappingObject, object data, string input)
		{
			var innerStatus = UnwrapContinuationData(wrappingObject, data);

			if (innerStatus == null)
				return Error();

			return innerStatus.Continuable.Continue(innerStatus.Data, input);
		}

		public ResultCode Code { get; }
		public ICollection<string>? Message { get; }
		public ContinuationStatus? ContinuationStatus { get; }

		public InputSpecs? InputSpecs { get; }
		public bool IsSuccess => Code == ResultCode.Success;
		public bool IsFailure => Code == ResultCode.Failure;
		public bool IsError => Code == ResultCode.Error;
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
