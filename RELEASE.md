# ShellInject 10.1.0 release

## Compatibility

The package validates its public API against the published 10.0.3 package during `dotnet pack`.
Existing overloads, parameter names, lifecycle overrides, extension wrappers, and public
argument-validation exception types remain available. The new APIs are opt-in; see README.

## Verify and package

Use .NET 10 with the corresponding MAUI workloads and native SDKs installed.

```sh
dotnet test ShellInjectTests/ShellInjectTests.csproj -c Release --collect:"XPlat Code Coverage" --settings ShellInjectTests/coverlet.runsettings
python3 ShellInjectTests/verify-coverage.py <new-report>/coverage.cobertura.xml
dotnet build ShellInject/ShellInject.csproj -c Release
dotnet build Sample/Sample.csproj -c Debug -f net10.0-android
dotnet pack ShellInject/ShellInject.csproj -c Release
```

Use the newly printed coverage report, not a previous TestResults directory. Unit tests
exercise MAUI model objects and toolkit popup state but do not validate native startup or rendering.
Before release, smoke-test the sample on the supported target devices, including initial
ViewModel binding, forward/reverse data, tab selection, and popup dismissal.

The existing project adds its Windows TFM only on Windows. The macOS package contains
net10.0, Android, iOS, and Mac Catalyst assets; validate Windows consumption separately.

Merge and tag the reviewed source before producing the final release artifacts so SourceLink
references the published commit. Publish the generated `ShellInject.10.1.0.nupkg` and matching
`.snupkg` from `ShellInject/bin/Release` using the repository owner's NuGet credentials.
No credentials belong in this repository. Packaging locally does not publish to NuGet.
