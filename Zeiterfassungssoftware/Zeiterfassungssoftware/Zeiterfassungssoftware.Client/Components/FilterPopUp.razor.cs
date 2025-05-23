using Microsoft.AspNetCore.Components;
using Zeiterfassungssoftware.Client.Data.Filter;

namespace Zeiterfassungssoftware.Client.Components
{
    public partial class FilterPopUp
    {
        public Type? Type => (Filter is StringFilter) ? typeof(StringFilterComponent) : (Filter is DateFilter) ? typeof(DateFilterComponent) : (Filter is TimeFilter) ? typeof(TimeFilterComponent) : null;

        [Parameter]
        public IFilter Filter { get; set; }

        private Dictionary<string, object> Parameters => new()
        {
            { "Filter", Filter }
        };

        public void ApplyFilter()
        {
            Filter.Enabled = true;
            Filter.PopUp = false;
        }

        public void Cancel()
        {
            Filter.PopUp = false;
        }
    }
}