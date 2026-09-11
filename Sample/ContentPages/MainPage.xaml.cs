using System.ComponentModel;
using Sample.Services;
using Sample.ViewModels;

namespace Sample.ContentPages;

public partial class MainPage : ContentPage
{
    private DemoSession? _session;
    private int _lastSeenResult;

    public MainPage()
    {
        InitializeComponent();
    }

    // UI-only behavior: reveal a newly delivered result rather than returning to a scrolled-away card.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_session is not null) _session.PropertyChanged -= OnSessionChanged;
        _session = (BindingContext as MainViewModel)?.Session;
        if (_session is null) return;
        _session.PropertyChanged += OnSessionChanged;
        RevealNewResult();
    }

    protected override void OnDisappearing()
    {
        if (_session is not null) _session.PropertyChanged -= OnSessionChanged;
        base.OnDisappearing();
    }

    private void OnSessionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DemoSession.ResultRevision)) RevealNewResult();
    }

    private void RevealNewResult()
    {
        if (_session is null || _session.ResultRevision == _lastSeenResult) return;
        _lastSeenResult = _session.ResultRevision;
        Dispatcher.Dispatch(() => _ = ScrollToResultAsync());
    }

    private async Task ScrollToResultAsync()
    {
        try { await DemoScroll.ScrollToAsync(ResultPanel, ScrollToPosition.Start, animated: false); }
        catch (Exception ex) { _session?.Record(nameof(MainPage), "Scroll interrupted", ex.Message); }
    }
}
