using Microsoft.AspNetCore.Components;

namespace Zeiterfassungssoftware.Client.Components
{
    public partial class Widget
    {

        [Parameter]
        public string Title { get; set; } = string.Empty;
        [Parameter]
        public string Text { get; set; } = string.Empty;
    }
}