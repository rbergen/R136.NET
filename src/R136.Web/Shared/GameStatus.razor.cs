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
		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Parameter]
		public string StatusText { get; set; }

		[Parameter]
		public EventCallback<string> TextSubmitted { get; set; }

		[Parameter]
		public bool Visible
		{
			get => _visible;
			set
			{
				if (_visible == value)
					return;

				_visible = value;

				if (VisibleChanged.HasDelegate)
					VisibleChanged.InvokeAsync(_visible);
			}
		}

		[Parameter]
		public EventCallback<bool> VisibleChanged { get; set; }

		[Parameter]
		public Tab? ShowTab
		{
			get => _displayedTab;
			set
			{
				if (_displayedTab == value)
					return;

				_displayedTab = value;
				
				if (_displayedTab.HasValue)
					_selectedTab = _displayedTab.Value;
			}
		}

#pragma warning disable IDE0044 // Add readonly modifier
		private string _enteredText = string.Empty;
#pragma warning restore IDE0044 // Add readonly modifier
		private Tab _selectedTab = Tab.Copy;
		private bool _visible = false;
		private Tab? _displayedTab = null;

		private async Task CopyTabClicked(MouseEventArgs e)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			_selectedTab = Tab.Copy;
		}

		private async Task PasteTabClicked(MouseEventArgs e)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.closeTooltips");
			_selectedTab = Tab.Paste;
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
			string text = _enteredText;

			_selectedTab = Tab.Copy;
			_enteredText = string.Empty;

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
