using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.ViewModels;

public class MimicPageViewModel(
    MimicViewModel mimic
)
    : PageViewModel
{
    public override string Header => "Mimic";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.PersonSync };

    public MimicViewModel Mimic { get; } = mimic;
}
