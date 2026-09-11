using CommunityToolkit.Mvvm.Input;
using Sample.Services;

namespace Sample.ViewModels;

public partial class ActivityViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private void Clear() => Session.Events.Clear();
}
