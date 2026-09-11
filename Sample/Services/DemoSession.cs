using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Sample.Models;

namespace Sample.Services;

// A single, bounded activity feed shared by the sample's screens. It is not a navigation service.
public partial class DemoSession : ObservableObject
{
    public ObservableCollection<ActivityEntry> Events { get; } = [];
    [ObservableProperty] private string _reference = "SHIP-2048";
    [ObservableProperty] private string _latestResult = "Your result will appear here after you finish a workflow.";
    [ObservableProperty] private string _resultSource = "READY FOR YOUR FIRST RUN";
    [ObservableProperty] private int _resultRevision;

    public string ValidReference => string.IsNullOrWhiteSpace(Reference) ? "SHIP-2048" : Reference.Trim();

    public void Record(string source, string eventName, object? detail = null)
    {
        void Add()
        {
            Events.Insert(0, new ActivityEntry(DateTime.Now.ToString("HH:mm:ss"), source, eventName, detail?.ToString() ?? "No parameter"));
            while (Events.Count > 60) Events.RemoveAt(Events.Count - 1);
        }

        if (MainThread.IsMainThread) Add();
        else MainThread.BeginInvokeOnMainThread(Add);
    }

    public void Receive(string callback, object? result)
    {
        LatestResult = result?.ToString() ?? "No data was returned.";
        ResultSource = callback;
        ResultRevision++;
        Record("MainViewModel", callback, result);
    }
}
