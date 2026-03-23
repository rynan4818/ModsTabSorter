using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ModsTabSorter.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        [NonNullable, UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> OrderedTabs { get; set; } = new List<string>();

        public virtual void Changed()
        {
        }

        public virtual void CopyFrom(PluginConfig other)
        {
            OrderedTabs = other?.OrderedTabs?.ToList() ?? new List<string>();
        }
    }
}
