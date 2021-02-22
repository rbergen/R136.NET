using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using R136.Web.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R136.Web.Shared
{
	public partial class ShowContentLog
	{
		private string _blockID = null;

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Parameter]
		public MarkupContentLog ContentLog { get; set; }

		[Parameter]
		public string BlockIDStart { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (_blockID == null)
				return;

			await JSRuntime.InvokeAsync<bool>("scrollToElementId", _blockID);
		}
	}
}
