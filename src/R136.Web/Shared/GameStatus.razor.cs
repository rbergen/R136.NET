using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Web;

namespace R136.Web.Shared
{
	public partial class GameStatus
	{
#pragma warning disable IDE0044 // Add readonly modifier
		private string enteredText = string.Empty;
#pragma warning restore IDE0044 // Add readonly modifier
		private Tab selectedTab = Tab.Copy;
		private bool visible = false;
		private Tab? displayedTab = null;

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Parameter]
		public string StatusText { get; set; }

		[Parameter]
		public EventCallback<string> TextSubmitted { get; set; }

		#pragma warning disable BL0007
        [Parameter]
		public bool Visible
		{
			get => this.visible;
			set
			{
				if (this.visible == value)
					return;

				this.visible = value;

				if (VisibleChanged.HasDelegate)
					VisibleChanged.InvokeAsync(this.visible);
			}
		}

		[Parameter]
		public Tab? ShowTab
		{
			get => this.displayedTab;
			set
			{
				if (this.displayedTab == value)
					return;

				this.displayedTab = value;
				
				if (this.displayedTab.HasValue)
					this.selectedTab = this.displayedTab.Value;
			}
		}
		#pragma warning restore BL0007

        [Parameter]
        public EventCallback<bool> VisibleChanged { get; set; }

        private async Task CopyTabClicked(MouseEventArgs e)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			this.selectedTab = Tab.Copy;
		}

		private async Task PasteTabClicked(MouseEventArgs e)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			this.selectedTab = Tab.Paste;
		}

		private async Task CopyClicked(MouseEventArgs e)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			await JSRuntime.InvokeVoidAsync("R136JS.setClipboard", StatusText);
		}

		private async Task LinkClicked(MouseEventArgs e)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			await JSRuntime.InvokeVoidAsync("R136JS.setClipboard", $"{NavigationManager.BaseUri}play/{HttpUtility.UrlEncode(StatusText)}");
		}

		private async Task SubmitInput(EventArgs e)
		{
			string text = this.enteredText;

			this.selectedTab = Tab.Copy;
			this.enteredText = string.Empty;

			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			Visible = false;

			if (TextSubmitted.HasDelegate)
				await TextSubmitted.InvokeAsync(text);
		}

		private async Task CancelClicked(MouseEventArgs e)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			Visible = false;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
			=> await JSRuntime.InvokeVoidAsync("R136JS.enableTooltips");

		public enum Tab
		{
			Copy,
			Paste
		}
	}
}
