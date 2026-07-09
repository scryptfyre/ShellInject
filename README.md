# ShellInject

ShellInject is a small .NET MAUI library for apps that use `Shell` and MVVM. It removes the repetitive parts of Shell navigation: route registration, ViewModel creation, passing data forward, returning data back, and calling common page lifecycle hooks from your ViewModels.

The goal is simple: call `UseShellInject()` once, write normal MAUI pages and ViewModels, and navigate without string routes or code-behind plumbing.

## What ShellInject Does

- Binds pages and popups to ViewModels automatically by naming convention.
- Keeps the existing XAML `ViewModelType` binding available for explicit mappings.
- Resolves ViewModels from MAUI dependency injection, or creates them with `ActivatorUtilities` when they are not registered.
- Registers Shell routes automatically when navigating by page type.
- Passes data into destination ViewModels through `DataReceivedAsync`.
- Returns data to previous pages through `ReverseDataReceivedAsync`.
- Provides lifecycle hooks for appearing, disappearing, first initialization, and post-navigation appear logic.
- Supports Shell pushes, multi-page stacks, modals, modal navigation stacks, tabs, flyout replacement, and CommunityToolkit popups.
- Keeps the older `Shell` extension methods available for compatibility while recommending the newer `ShellNavigation` API.

## Requirements

- A .NET MAUI Shell-based app.
- .NET 10 target frameworks, matching this package version.
- No separate `CommunityToolkit.Maui` package reference is required unless your app directly depends on toolkit APIs. ShellInject brings its required package dependencies transitively.

For normal Shell navigation without popups, setup is just:

```csharp
builder
    .UseMauiApp<App>()
    .UseShellInject();
```

If you use ShellInject popup APIs, register the MAUI Community Toolkit during startup:

```csharp
builder
    .UseMauiApp<App>()
    .UseMauiCommunityToolkit()
    .UseShellInject();
```

If you do not use popups or other toolkit features, do not add `UseMauiCommunityToolkit()` just for ShellInject navigation.

## Installation

Install the package from NuGet:

```bash
dotnet add package ShellInject
```

Then enable it in `MauiProgram.cs`:

```csharp
using ShellInject;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseShellInject();

        return builder.Build();
    }
}
```

That is the only required ShellInject setup for the default experience.

## ViewModel Binding

ShellInject supports two binding styles. You can use either one in the same app.

### Option 1: Convention Binding

Convention binding is enabled by default from `UseShellInject()`.

```text
MainPage       -> MainViewModel
DetailsPage    -> DetailsViewModel
SamplePage3    -> SamplePage3ViewModel
SamplePopup    -> SamplePopupViewModel
```

For a page like this:

```csharp
namespace MyApp.Pages;

public partial class DetailsPage : ContentPage
{
    public DetailsPage()
    {
        InitializeComponent();
    }
}
```

ShellInject looks for a ViewModel named `DetailsViewModel`, then falls back to `DetailsPageViewModel`. If both exist, `DetailsViewModel` wins.

```csharp
namespace MyApp.ViewModels;

public class DetailsViewModel : ShellInjectViewModel
{
}
```

You should still use `x:DataType` in XAML for compiled bindings:

```xml
<ContentPage
    x:Class="MyApp.Pages.DetailsPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:vm="clr-namespace:MyApp.ViewModels"
    x:DataType="vm:DetailsViewModel">

    <Label Text="{Binding Title}" />
</ContentPage>
```

Convention binding never replaces an existing `BindingContext`. If your app sets the binding manually, ShellInject leaves it alone.

### Option 2: Explicit XAML Binding

If a page does not follow the naming convention, or if you want the mapping to be obvious in XAML, use the existing attached property:

```xml
<ContentPage
    x:Class="MyApp.Pages.OrderDetailsPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:extensions="clr-namespace:ShellInject.Extensions;assembly=ShellInject"
    xmlns:vm="clr-namespace:MyApp.ViewModels"
    extensions:ShellInjectPageExtensions.ViewModelType="{x:Type vm:OrderEditorViewModel}"
    x:DataType="vm:OrderEditorViewModel">
</ContentPage>
```

Explicit `ViewModelType` mappings take precedence over convention binding.

### Configuring Convention Binding

You can disable convention binding or customize suffixes at startup:

```csharp
builder.UseShellInject(options =>
{
    options.AutoBindViewModelsByConvention = true;
    options.PageSuffix = "Page";
    options.ViewModelSuffix = "ViewModel";
});
```

To keep only explicit XAML binding:

```csharp
builder.UseShellInject(options =>
{
    options.AutoBindViewModelsByConvention = false;
});
```

## ViewModel Base Class

Inherit from `ShellInjectViewModel` when you want lifecycle hooks and navigation data.

```csharp
using ShellInject;

public class DetailsViewModel : ShellInjectViewModel
{
    public override Task InitializedAsync()
    {
        // Runs once for this ViewModel instance.
        return Task.CompletedTask;
    }

    public override Task DataReceivedAsync(object? parameter)
    {
        // Runs when this page receives forward navigation data.
        return Task.CompletedTask;
    }

    public override Task ReverseDataReceivedAsync(object? parameter)
    {
        // Runs when another page returns data to this ViewModel.
        return Task.CompletedTask;
    }

    public override Task OnAppearedAsync()
    {
        // Runs after Shell navigation completes.
        return Task.CompletedTask;
    }

    public override void OnAppearing()
    {
        // Runs when the page appears.
    }

    public override void OnDisAppearing()
    {
        // Runs when the page disappears.
    }
}
```

The hooks are intentionally lightweight. Put page behavior in the ViewModel, keep code-behind focused on `InitializeComponent()`, and let ShellInject handle the navigation plumbing.

## Dependency Injection

ShellInject uses the MAUI service provider configured in `MauiProgram.cs`.

```csharp
builder.Services.AddSingleton<IOrdersService, OrdersService>();
builder.Services.AddTransient<DetailsViewModel>();
```

When a ViewModel is needed, ShellInject resolves it from DI if it is registered. If it is not registered, ShellInject uses `ActivatorUtilities`, so constructor injection still works when the dependencies are registered.

```csharp
public class DetailsViewModel(IOrdersService ordersService) : ShellInjectViewModel
{
}
```

If no service provider is available yet, ShellInject falls back to a parameterless constructor.

You can also resolve services directly when needed:

```csharp
var required = Injector.GetRequiredService<IOrdersService>();
var optional = Injector.GetService<IOrdersService>();
```

## Navigation API

Use `ShellNavigation` for new code. It defaults to `Shell.Current`, and every method also accepts an explicit `Shell` for multi-window or advanced scenarios.

```csharp
await ShellNavigation.PushAsync<DetailsPage>(parameter: orderId);
await ShellNavigation.PushAsync<DetailsPage>(shell: myShell, parameter: orderId);
```

### Push a Page

```csharp
await ShellNavigation.PushAsync<DetailsPage>(parameter: orderId);
```

ShellInject registers a route for `DetailsPage` automatically, navigates to it, binds the ViewModel, and calls `DataReceivedAsync(orderId)` on the destination ViewModel.

### Return Data

```csharp
await ShellNavigation.PopAsync(parameter: "Saved");
```

The previous page receives the value in `ReverseDataReceivedAsync`.

```csharp
public override Task ReverseDataReceivedAsync(object? parameter)
{
    StatusMessage = parameter as string ?? string.Empty;
    return Task.CompletedTask;
}
```

`PopAsync` works for regular Shell pages and modal pages. ShellInject detects the current navigation context and uses the correct pop operation.

### Push Multiple Pages

```csharp
await ShellNavigation.PushMultiStackAsync(
    pageTypes: [typeof(DetailsPage), typeof(StepTwoPage), typeof(StepThreePage)],
    parameter: orderId);
```

This builds a stack in one call. The final page receives the parameter after navigation completes.

To close that regular Shell stack and return data to the root page, use:

```csharp
await ShellNavigation.PopToRootAsync(parameter: "Finished");
```

### Modals

Push a modal page by type:

```csharp
await ShellNavigation.PushModalAsync<DetailsPage>(parameter: orderId);
```

Or push a page instance inside a modal `NavigationPage`:

```csharp
await ShellNavigation.PushModalWithNavigationAsync(
    page: new StepOnePage(),
    parameter: orderId);
```

Close the current modal page and return data:

```csharp
await ShellNavigation.PopAsync(parameter: "Saved from modal");
```

Close the entire modal navigation stack:

```csharp
await ShellNavigation.PopModalStackAsync(data: "Closed modal stack");
```

Use `PopModalStackAsync` only for modal navigation stacks. For regular Shell stacks, use `PopToRootAsync`, `PopToAsync`, or `PopAsync`.

### Pop to a Specific Page

```csharp
await ShellNavigation.PopToAsync<MainPage>(parameter: "Back to main");
```

ShellInject searches the navigation stack for the target page type and delivers the value through `ReverseDataReceivedAsync`.

### Send Data to a Page Already on the Stack

```csharp
await ShellNavigation.SendDataToPageAsync<MainPage>(data: "Refresh now");
```

This is useful when a page should receive data without making it the result of a pop operation.

### Replace Flyout Content

```csharp
await ShellNavigation.ReplaceAsync<OrdersPage>(parameter: filter);
```

`ReplaceAsync` is intended for pages already present in the Shell visual hierarchy, such as Flyout items. It navigates by the target page type name and delivers data after the replacement.

### Change Tabs

```csharp
await ShellNavigation.ChangeTabAsync(
    tabIndex: 1,
    parameter: "Selected tab two");
```

ShellInject can select a tab in the current Shell section or locate a matching tab section elsewhere in the Shell hierarchy, then deliver data to the selected tab ViewModel.

### Popups

Popup support uses `CommunityToolkit.Maui`. ShellInject already declares the NuGet dependency, but popup support still requires toolkit startup registration:

```csharp
builder.UseMauiCommunityToolkit();
```

```csharp
await ShellNavigation.ShowPopupAsync<OrderPopup>(data: orderId);
```

The popup ViewModel receives the value in `DataReceivedAsync`.

```csharp
await ShellNavigation.DismissPopupAsync<OrderPopup>(data: "Popup closed");
```

The current page receives the returned value in `ReverseDataReceivedAsync`.

## API Reference

Common `ShellNavigation` methods:

```csharp
ShellNavigation.PushAsync<TPageType>(shell, parameter, animate);
ShellNavigation.PushMultiStackAsync(shell, pageTypes, parameter, animate, animateAllPages);
ShellNavigation.PushModalAsync<TPageType>(shell, parameter, animate);
ShellNavigation.PushModalWithNavigationAsync(shell, page, parameter, animate);
ShellNavigation.PopAsync(shell, parameter, animate);
ShellNavigation.PopModalStackAsync(shell, data, animate);
ShellNavigation.PopToAsync<TPageType>(shell, parameter);
ShellNavigation.PopToRootAsync(shell, parameter, animate);
ShellNavigation.ChangeTabAsync(shell, tabIndex, parameter, popToRootFirst);
ShellNavigation.ReplaceAsync<TPageType>(shell, parameter, animate);
ShellNavigation.SendDataToPageAsync<TPageType>(shell, data);
ShellNavigation.ShowPopupAsync<TPopup>(shell, data, onError);
ShellNavigation.DismissPopupAsync<TPopup>(shell, data);
```

All `shell` parameters are optional. If omitted, ShellInject uses `Shell.Current`.

## Backwards Compatibility

Earlier versions exposed navigation primarily as extension methods on `Shell`:

```csharp
await Shell.Current.PushAsync<DetailsPage>(parameter);
await Shell.Current.PopAsync(parameter);
await Shell.Current.ShowPopupAsync<OrderPopup>(data);
```

Those methods are still available. They have not been removed. They are marked `[Obsolete]` with `false`, which means existing apps get compiler warnings but do not fail to build.

New code should use `ShellNavigation`:

```csharp
await ShellNavigation.PushAsync<DetailsPage>(parameter: parameter);
```

The old methods forward into the new implementation, so existing apps receive the same routing, binding, lifecycle, modal, tab, and popup fixes while they migrate at their own pace.

## Practical Notes

- `UseShellInject()` should be called during MAUI startup before the app is built.
- ShellInject is designed for Shell-based apps. If your app does not use `Shell`, pass an explicit `Shell` is not enough; the app still needs a Shell navigation structure.
- Convention binding is convenient, but explicit XAML `ViewModelType` is the right choice for unusual page/ViewModel names.
- Keep `x:DataType` even when ShellInject sets `BindingContext`; it gives you compiled binding checks and better performance.
- If two pages share the same class name in different namespaces, ShellInject falls back to namespace-qualified generated routes.
- Missing convention matches are ignored. ShellInject does not throw just because a page has no matching ViewModel.

## Troubleshooting

### Shell not found

If you see an `InvalidOperationException` saying Shell could not be found, make sure your app creates a `Shell` root page:

```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    return new Window(new AppShell());
}
```

For multi-window apps, pass the correct Shell instance directly:

```csharp
await ShellNavigation.PushAsync<DetailsPage>(shell: myShell, parameter: data);
```

### ViewModel is not binding

Check the page and ViewModel names first:

```text
DetailsPage -> DetailsViewModel
DetailsPage -> DetailsPageViewModel
```

If the names do not match, use the XAML `ViewModelType` override. Also check that an existing `BindingContext` is not already set, because convention binding will not replace it.

### Trimmed or NativeAOT builds

Convention binding discovers ViewModels by type name at runtime. If your release build uses aggressive trimming or NativeAOT, make sure your ViewModel types and constructors are preserved, or use the explicit XAML `ViewModelType` binding for those pages. Explicit `ViewModelType` references give the linker a stronger static reference than convention-only discovery.

### Data is not received

Make sure the ViewModel inherits from `ShellInjectViewModel` or implements `IShellInjectShellViewModel`. Forward navigation data is delivered to `DataReceivedAsync`; returned data is delivered to `ReverseDataReceivedAsync`.

### Modal stack does not close

Use `PopModalStackAsync` for modal navigation stacks created with `PushModalWithNavigationAsync`. Use `PopToRootAsync`, `PopToAsync`, or `PopAsync` for normal Shell stacks.

## Sample Project

The repository includes a sample MAUI app that demonstrates:

- Convention ViewModel binding.
- Explicit XAML `ViewModelType` binding.
- Constructor injection into ViewModels.
- Push navigation and returned data.
- Modal pages and modal navigation stacks.
- Multi-page Shell stacks.
- Popup data flow.
- Tab selection and flyout replacement.
- Backwards-compatible APIs through deprecated Shell extension methods.

## Build and Test

Useful commands for contributors:

```bash
dotnet build ShellInject/ShellInject.csproj -c Release
dotnet test ShellInjectTests/ShellInjectTests.csproj -c Release
dotnet build Sample/Sample.csproj -c Debug -f net10.0-android
```

Coverage can be collected with:

```bash
dotnet test ShellInjectTests/ShellInjectTests.csproj -c Release --collect:"XPlat Code Coverage"
```
