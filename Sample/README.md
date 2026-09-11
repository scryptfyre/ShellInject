# ShellInject Navigation Lab

A runnable, local-only sample for learning ShellInject through complete workflows.
The UI supports light/dark themes, compiled bindings, scrollable phone layouts, and
bounded reading widths on tablets. No account, network service, or backend is required.

## Run

Install .NET 10 and the MAUI workload/native SDK for your target, then open `Sample.csproj`
in a MAUI-capable IDE and select an Android emulator or iOS simulator/device.

```sh
dotnet build Sample/Sample.csproj -c Debug -f net10.0-android
```

The project targets Android and iOS, plus Windows when built on Windows. For an iOS simulator:

```sh
dotnet build Sample/Sample.csproj -c Debug -f net10.0-ios -t:Run
```

## Start in the lab

1. Change the shipment reference, or use `SHIP-2048`. Blank input uses that default.
2. Start a workflow using its button. The flyout and native tab headers navigate normally;
   they do not create a new ShellInject payload.
3. Read the destination's incoming-data card and the API snippet.
4. Finish using the demonstrated return/close action. Check the **Result inbox** on the lab.
5. Open **Activity** in the toolbar to inspect actual API calls, callbacks, and data.
   Events are newest first, capped at 60, and stored only for this app session.

**Setup** in the toolbar explains startup, all three binding styles, DI, lifecycle hooks,
diagnostics, and compatibility. Each screen includes its relevant source file names.

## Workflow map

| Workflow | Try this | What to observe | Source |
|---|---|---|---|
| Typed page push | Open Details, edit the reply, return | `DemoRequest` arrives in a typed hook; `DemoResult` returns to MainViewModel | `MainViewModel.cs`, `DetailsViewModel.cs`, `Models/DemoRequest.cs` |
| Direct update | In pushed Details, send without leaving | MainViewModel receives `ReverseDataReceivedAsync` while Details stays visible | `DetailsViewModel.SendUpdateAsync` |
| Standalone modal | Open Details as a modal, return | `PopAsync` detects the modal context; the lab receives the result | `MainViewModel.OpenModalAsync` |
| Popup | Confirm a reply; repeat and cancel | Confirm sends data, cancel/outside dismissal does not | `SamplePopup.xaml`, `SamplePopupViewModel.cs` |
| Shell multi-stack | Open the stack and pop one step | Only Review gets the initial payload; Prepare gets the reverse result | `SamplePage3ViewModel.cs`, `StackStepViewModel.cs` |
| Root return | In Review, finish | `PopToRootAsync` clears the regular Shell stack and returns a result | `SamplePage3ViewModel.FinishAsync` |
| Return to a type | In Review, return to MainPage | `PopToAsync<MainPage>` finds the existing page on the regular stack | `SamplePage3ViewModel.PopToLabAsync` |
| Modal navigation | Open the modal stack, review, approve | Native `INavigation.PushAsync` stays inside the modal; `PopAsync` returns data to its root | `ModalStackViewModel.cs`, `ModalStepViewModel.cs` |
| Close a modal stack | Close from Prepare or Review | `PopModalStackAsync` closes the modal stack and sends data to the lab | `ModalStackViewModel.CloseStackAsync`, `ModalStepViewModel.CloseAllAsync` |
| Tabs by index | Send to Inbox | `ChangeTabAsync(tabIndex: 1)` delivers forward data | `TabbedPageOneViewModel.cs`, `MainViewModel.cs` |
| Tabs by type | In Inbox, send to Overview | `ChangeTabAsync<TabbedPageOne>` finds materialized content | `InboxViewModel.cs`, `AppShell.xaml` |
| Flyout replacement | Replace with Dispatch, then return | No details push; both directions deliver forward data using `ReplaceAsync` | `FlyoutPageThreeViewModel.cs`, `AppShell.xaml` |

### Navigation contexts matter

- `ShellNavigation.PushAsync` targets the regular Shell stack. It does **not** push inside
  a modal `NavigationPage`. The modal example gets the active modal's `INavigation` explicitly
  for its internal push. That native push does not deliver ShellInject forward parameters.
- `PopToRootAsync` / `PopToAsync` operate on the regular Shell stack. Use `PopModalStackAsync`
  to finish a modal navigation flow.
- `SendDataToPageAsync` searches the regular Shell stack. Its button is offered only in
  the pushed Details workflow, not the modal variant.
- `ReplaceAsync<T>` targets an existing Shell destination with a route matching the page
  type name. `MainPage` and `FlyoutPageThree` have explicit routes in `AppShell.xaml`.
  Dispatch is materialized in the hierarchy so its page exists when the library delivers
  the post-navigation parameter, including on the first visit.
- Typed tab selection searches already materialized content. Both sample tabs are created
  in `AppShell.xaml` to make this predictable; the library does not instantiate arbitrary
  templates to search for a type.
- Native Back closes a page without a new return payload. Previously received results
  deliberately remain in the result inbox; cancellation is not presented as a new success.

## Binding and DI examples

- **Convention:** `DetailsPage → DetailsViewModel`; the typed VM derives from
  `ShellInjectViewModel<DemoRequest>` and receives `ISampleService` through its constructor.
- **Registration:** `SamplePage2 → StackStepViewModel` via
  `options.RegisterViewModel<SamplePage2, StackStepViewModel>()` in `MauiProgram.cs`.
- **Explicit XAML:** `TabbedPageTwo → InboxViewModel` through `ViewModelType`.
- **Reusable ContentView:** the setup guide's `BindingInfoView` has its own explicitly mapped
  `BindingInfoViewModel`. It demonstrates DI outside pages; it does not receive page navigation data.
- **Existing BindingContext:** explicit contexts remain owned by the app. Do not assign one
  just to activate convention binding; doing so tells ShellInject to leave it alone.
- `DemoSession` and `ISampleService` are singletons. `DetailsViewModel` is registered transient;
  other unregistered ViewModels use ShellInject's DI activation fallback.

Page constructors only call `InitializeComponent()`. MainPage's code-behind handles one UI-only
concern: scrolling a newly received result into view. The shared `BaseViewModel` centralizes
busy/error UI and lifecycle logging; actual navigation calls stay in each demo's ViewModel.
`DetailsViewModel` uses the library's typed base directly so the typed dispatch is visible.
`x:DataType` is present on pages and data templates for compiled bindings.

## Diagnostics and limitations

`MauiProgram.cs` installs `options.ErrorHandler` to record recovered library failures. Commands
catch their own exceptions, show an inline message, and add an Activity event. Buttons in each
workflow are guarded against overlapping actions; popup dismissal runs on its own ViewModel
while the opener awaits the popup's lifetime.

The Activity screen records actual callbacks, not a prescribed order. Async lifecycle callbacks
can continue after the navigation Task completes. Read incoming data in `DataReceivedAsync`,
not `InitializedAsync`. Some native navigation actions do not run ShellInject's async pipeline.

This is a **single-window** sample. In a multi-window app pass `shell: owningShell` consistently,
especially when showing/dismissing a popup. Explicit registrations reduce reflection discovery;
they do not establish complete NativeAOT support. Test your own trimmed publish on device.

New examples use `ShellNavigation`. The library's obsolete Shell extension wrappers and legacy
`OnDisAppearing` hook remain available for existing consumers; see the root README for migration.

## Manual verification checklist

- [ ] Change the reference and confirm it arrives in Details, popup, tabs, and Dispatch.
- [ ] Edit a result in Details; send it without leaving; then return it to the lab.
- [ ] Open standalone modal Details; return with data; repeat with system Back.
- [ ] Confirm, cancel, and tap outside a popup; reopen and verify no stale popup is dismissed.
- [ ] Pop one Shell-stack step, continue again, then test both root and typed-page returns.
- [ ] In the modal stack approve to its root, close to the lab, reopen and close from the nested step.
- [ ] Switch tabs by index, by type, and by native header; only API buttons send fresh data.
- [ ] Replace with Dispatch and back; verify forward callbacks rather than reverse callbacks.
- [ ] Read Setup, including the DI-backed ContentView; inspect and clear Activity.
- [ ] Repeat in light/dark mode, with larger system text, and on a narrow phone and tablet.
