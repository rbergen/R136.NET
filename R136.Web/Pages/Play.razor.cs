using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using R136.Interfaces;
using R136.Web.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R136.Web.Pages
{
	public partial class Play
	{
		private string _input = string.Empty;
		private object _continuationData = null;
		private InputSpecs _inputSpecs;
		private MarkupString? _error = null;
		private bool _ended = false;
		private bool _pauseBeforeRoomStatus = false;
		private bool _pause = false;

		[Inject] 
		public IEngine Engine { get; set; }

		[Inject]
		public MarkupContentLog ContentLog { get; set; }

		protected override void OnInitialized()
			=> CycleEngine();

		private void ProceedClicked()
		{
			_pause = false;
			CycleEngine();
		}

		private void CycleEngine()
		{
			var proceed = true;
			
			while (proceed)
			{
				switch (Engine.DoNext)
				{
					case NextStep.ShowStartMessage:
						ContentLog.Add(ContentBlockType.StartMessage, Engine.StartMessage);

						break;

					case NextStep.ShowRoomStatus:
						if (_pauseBeforeRoomStatus)
						{
							_pause = true;
							_pauseBeforeRoomStatus = false;
							proceed = false;
						}
						else
							ContentLog.Add(ContentBlockType.RoomStatus, Engine.RoomStatus);

						break;

					case NextStep.ProgressAnimateStatus:
						ContentLog.Add(ContentBlockType.AnimateStatus, Engine.ProgressAnimateStatus());

						break;

					case NextStep.RunCommand:
						_inputSpecs = Engine.CommandInputSpecs;
						StateHasChanged();
						proceed = false;

						break;
				}
			}
		}

		private void SubmitInput(EventArgs e)
		{
			_error = null;

			var result = _continuationData != null
				? Engine.Continue(_continuationData, ApplyInputSpecs(_input))
				: Engine.Run(_input);

			if (result.IsError)
			{
				_error = result.Message != null ? result.Message.ToMarkupString() : (MarkupString)"An unspecified error occurred";
				return;
			}

			ContentLog.Add(ContentBlockType.Input, _input);
			_input = string.Empty;

			if (ProcessResult(result, ContentBlockType.RunResult))
			{
				if (Engine.DoNext == NextStep.ProgressAnimateStatus)
					_pauseBeforeRoomStatus = true;

				CycleEngine();
			}
		}

		private string ApplyInputSpecs(string text)
		{
			if (_inputSpecs.IsLowerCase)
				text = text.ToLower();

			if (text.Length > _inputSpecs.MaxLength)
				text = text[..^(_inputSpecs.MaxLength - 1)];

			return text;
		}

		private bool ProcessResult(Result result, ContentBlockType blockType)
		{
			switch (result.Code)
			{
				case ResultCode.InputRequested:
					ContentLog.Add(blockType, result.Code, result.Message);
					_continuationData = result.ContinuationStatus.Data;

					return false;

				case ResultCode.EndRequested:
					_error = null;
					_continuationData = null;
					_ended = true;

					return false;

				case ResultCode.Success:
				case ResultCode.Failure:
					ContentLog.Add(blockType, result.Code, result.Message);

					return true;
			}

			return true;
		}
	}
}
