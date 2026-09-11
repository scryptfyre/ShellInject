# ShellInject sample

A .NET MAUI app that demonstrates every ShellInject navigation workflow. It runs entirely offline.

## Running

You need .NET 10 and the MAUI workload for your platform. Open `ShellInject.sln` in Visual Studio or Rider and run the `Sample` project, or build from the command line:

```bash
# Android
dotnet build Sample/Sample.csproj -f net10.0-android -t:Run

# iOS simulator
dotnet build Sample/Sample.csproj -f net10.0-ios -t:Run
```

The sample targets Android and iOS, and also Windows when built on Windows. It references the library project directly, so changes to `ShellInject` show up the next time you run it.

## Using the app

The home screen lists each workflow as a card with its `ShellNavigation` call and a button to try it. Change the reference number at the top to follow your own value through each example. When a workflow returns a result, it appears in **Result inbox**.

Two toolbar buttons open reference screens:

- **Setup** covers startup, the three binding styles, DI, lifecycle methods, and error handling.
- **Activity** logs every navigation call, lifecycle callback, and parameter in the order they happened. This is the easiest way to see when each method runs.

Only the buttons inside the app send data. Switching tabs or flyout items yourself works like normal Shell navigation and doesn't pass a parameter.

## Workflows

| Workflow | API | Code |
|---|---|---|
| Push a page with a typed object and return a result | `PushAsync<DetailsPage, DemoRequest>`, `PopAsync` | `MainViewModel`, `DetailsViewModel` |
| Update the previous page without leaving | `SendDataToPageAsync<MainPage>` | `DetailsViewModel` |
| Open the same page as a modal | `PushModalAsync<DetailsPage, DemoRequest>` | `MainViewModel` |
| Popup with confirm and cancel | `ShowPopupAsync`, `DismissPopupAsync` | `MainViewModel`, `SamplePopupViewModel` |
| Two-page Shell stack | `PushMultiStackAsync`, `PopAsync`, `PopToAsync<MainPage>`, `PopToRootAsync` | `MainViewModel`, `StackStepViewModel`, `SamplePage3ViewModel` |
| Modal with its own navigation stack | `PushModalWithNavigationAsync`, `PopModalStackAsync` | `ModalStackViewModel`, `ModalStepViewModel` |
| Select a tab by index or by page type | `ChangeTabAsync`, `ChangeTabAsync<TabbedPageOne>` | `TabbedPageOneViewModel`, `InboxViewModel` |
| Switch flyout items | `ReplaceAsync<FlyoutPageThree>`, `ReplaceAsync<MainPage>` | `FlyoutPageThreeViewModel` |

## Binding examples

| Page | ViewModel | How it's bound |
|---|---|---|
| `DetailsPage` | `DetailsViewModel` | Naming convention |
| `SamplePage2` | `StackStepViewModel` | `options.RegisterViewModel` in `MauiProgram.cs` |
| `TabbedPageTwo` | `InboxViewModel` | `ViewModelType` in XAML |
| `BindingInfoView` (a `ContentView`) | `BindingInfoViewModel` | `ViewModelType` in XAML |

`DetailsViewModel` derives from `ShellInjectViewModel<DemoRequest>` and gets `ISampleService` through its constructor. The other ViewModels derive from `BaseViewModel`, which handles busy state, inline error messages, and logging to the Activity screen. The navigation calls themselves stay in each ViewModel so they're easy to find.

## Project layout

```text
Sample/
├── AppShell.xaml          Flyout items, tabs, and routes
├── MauiProgram.cs         UseShellInject, registrations, ErrorHandler, DI
├── ContentPages/          Pages and the popup
├── ContentViews/          BindingInfoView
├── ViewModels/            One ViewModel per page
├── Models/                DemoRequest, DemoResult, ActivityEntry
├── Services/              DemoSession (shared state and activity log), ISampleService
└── Resources/Styles/      Colors and styles, with light and dark themes
```

## Things to notice

- The modal stack moves to its second page with the modal's own `Navigation.PushAsync`. `ShellNavigation.PushAsync` would push onto the Shell stack behind the modal.
- In `AppShell.xaml`, both tab pages and the Dispatch page are declared directly instead of with `ContentTemplate`. That way they already exist when `ChangeTabAsync<TPage>` and `ReplaceAsync` look for them.
- The **Send update** button only appears when Details was pushed, not opened as a modal. `SendDataToPageAsync` searches the Shell stack, and a modal isn't on it.
- Pressing the system back button closes a page without sending a result, so the inbox keeps its previous value.

## Manual test checklist

- [ ] Change the reference and check that it reaches Details, the popup, both tabs, and Dispatch.
- [ ] In Details, send an update, then return a result.
- [ ] Open Details as a modal and return a result. Open it again and use the back button instead.
- [ ] In the popup, confirm, cancel, and tap outside it.
- [ ] In the Shell stack, go back one step, continue, then return to root. Repeat and use "Return to MainPage by type".
- [ ] In the modal stack, approve back to the first step, then close the whole stack. Repeat and close from the second step.
- [ ] Select tabs by index, by type, and by tapping the tab bar.
- [ ] Switch to Dispatch and back.
- [ ] Check the Activity log, then clear it.
- [ ] Repeat in dark mode, with a larger text size, and on a tablet.
