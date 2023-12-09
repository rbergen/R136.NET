using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using R136.Web.Tools;
using System.Threading.Tasks;

namespace R136.Web.Shared
{
	public partial class ShowContentLog
	{
		private string blockID = null;

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Parameter]
		public MarkupContentLog ContentLog { get; set; }

		[Parameter]
		public string BlockIDStart { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (this.blockID == null)
				return;

			await JSRuntime.InvokeAsync<bool>("R136JS.scrollToElementId", this.blockID);
		}
	}
}
