using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace R136.Interfaces
{
	public enum ResultCode
	{
		Success,
		Failure,
		Error,
		InputRequested,
		EndRequested
	}

	public class Result
	{
		private static readonly Result _success = new Result(ResultCode.Success);
		private static readonly Result _failure = new Result(ResultCode.Failure);
		private static readonly Result _error = new Result(ResultCode.Error);

		public static Result Success() => _success;
		public static Result Success(StringValues message) => new Result(ResultCode.Success, message);
		public static Result Success(string message) => new Result(ResultCode.Success, new string[] { message });

		public static Result Failure() => _failure;
		public static Result Failure(StringValues message) => new Result(ResultCode.Failure, message);
		public static Result Failure(string message) => new Result(ResultCode.Failure, new string[] { message });

		public static Result Error() => _error;
		public static Result Error(StringValues message) => new Result(ResultCode.Error, message);
		public static Result Error(string message) => new Result(ResultCode.Error, new string[] { message });

		public static Result EndRequested() => new Result(ResultCode.EndRequested);
		public static Result EndRequested(StringValues message) => new Result(ResultCode.EndRequested, message);

		public static Result InputRequested(ContinuationStatus status, InputSpecs specs, StringValues message)
			=> new Result(status, specs, message);

		public Result WrapInputRequest(string key, int number, string[]? texts = null)
			=> WrapInputRequest(key, new int[] { number }, texts);

		public Result WrapInputRequest(string key, int[]? numbers = null, string[]? texts = null)
			=> IsInputRequest
			? InputRequested(new ContinuationStatus() { Key = key, InnerStatus = ContinuationStatus, Texts = texts, Numbers = numbers }, InputSpecs!, Message)
			: this;

		public static ContinuationStatus? UnwrapContinuationStatus(string key, ContinuationStatus status)
			=> key == status.Key ? status.InnerStatus : null;

		public static Result ContinueWrappedContinuationStatus(string key, ContinuationStatus status, string input, Func<ContinuationStatus, string, Result> continueInvoker)
		{
			var innerStatus = UnwrapContinuationStatus(key, status);

			if (innerStatus == null)
				return Error();

			return continueInvoker.Invoke(status, input);
		}

		public ResultCode Code { get; }
		public StringValues Message { get; }
		public ContinuationStatus? ContinuationStatus { get; }

		public InputSpecs? InputSpecs { get; }
		public bool IsSuccess => Code == ResultCode.Success;
		public bool IsFailure => Code == ResultCode.Failure;
		public bool IsError => Code == ResultCode.Error;
		public bool IsInputRequest => Code == ResultCode.InputRequested;
		public bool IsEndRequest => Code == ResultCode.EndRequested;

		public Result(ResultCode code)
			=> Code = code;

		public Result(ResultCode code, StringValues message)
		{
			if (code == ResultCode.InputRequested)
				throw new ArgumentException("ResultCode ContinuationRequested requires InputSpecs", nameof(code));

			(Code, Message) = (code, message);
		}

		public Result(ContinuationStatus status, InputSpecs specs, StringValues message)
			=> (Code, ContinuationStatus, InputSpecs, Message)
			= (ResultCode.InputRequested, status, specs, message);
	}
}
