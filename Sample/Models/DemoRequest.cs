namespace Sample.Models;

// A real object travels by reference through ShellInject; no query-string serialization is needed.
public sealed record DemoRequest(string Reference, string Note, bool IsModal = false);

public sealed record DemoResult(string Reference, string Message)
{
    public override string ToString() => $"{Reference} · {Message}";
}

public sealed record ActivityEntry(string Time, string Source, string Event, string Detail);
