# ShellInject

[![NuGet](https://img.shields.io/nuget/v/ShellInject.svg)](https://www.nuget.org/packages/ShellInject)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/scryptfyre/ShellInject/blob/main/LICENSE)

ShellInject handles the plumbing around .NET MAUI Shell navigation in MVVM apps. You navigate by page type, pass real objects between ViewModels, and get lifecycle callbacks in the ViewModel instead of the page's code-behind.

```csharp
// MainViewModel
await ShellNavigation.PushAsync<OrderPage, Order>(order);

// OrderViewModel
public override Task DataReceivedAsync(Order? order) { ... }

// Send a result back to MainViewModel.ReverseDataReceivedAsync
await ShellNavigation.PopAsync(parameter: "Saved");
```

## Features

- Navigate by page type. Routes are registered for you.
- Pages and popups are bound to ViewModels by naming convention, by an attribute in XAML, or by registration at startup.
- ViewModels are resolved from the MAUI service provider, so constructor injection works.
- Pass any object forward (`DataReceivedAsync`) or back (`ReverseDataReceivedAsync`). Nothing is serialized into a query string.
- Works with pushes, multi-page stacks, modals, modal navigation stacks, tabs, flyout items, and CommunityToolkit popups.

## Requirements

- .NET 10 with the .NET MAUI workload
- An app that uses `Shell` as its root page

The package targets `net10.0`, Android, iOS, and Mac Catalyst.

## Installation

```bash
dotnet add package ShellInject
```

`CommunityToolkit.Maui` and `CommunityToolkit.Mvvm` come in as dependencies, so you don't need to add them yourself.

## Getting started

### 1. Register ShellInject

```csharp
using ShellInject;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // only needed if you use popups
            .UseShellInject();

        builder.Services.AddSingleton<IOrderService, OrderService>();

        return builder.Build();
    }
}
```

Make sure your app's window uses a `Shell`:

```csharp
protected override Window CreateWindow(IActivationState? activationState)
    => new(new AppShell());
```

### 2. Create a page and a ViewModel

```csharp
public partial class OrderPage : ContentPage
{
    public OrderPage() => InitializeComponent();
}
```

```csharp
public class OrderViewModel(IOrderService orders) : ShellInjectViewModel<Order>
{
    public override async Task DataReceivedAsync(Order? order)
    {
        // Called with the object passed to PushAsync
    }
}
```

`OrderPage` is bound to `OrderViewModel` automatically. Keep `x:DataType="vm:OrderViewModel"` in your XAML so you still get compiled bindings.

### 3. Navigate

```csharp
await ShellNavigation.PushAsync<OrderPage, Order>(order);
```

## Binding pages to ViewModels

There are three ways to connect a page (or popup) to its ViewModel. You can mix them in the same app.

### Naming convention

This is on by default. For a page named `OrderPage`, ShellInject looks for `OrderViewModel`, then `OrderPageViewModel`. It checks the matching `ViewModels` namespace first (`MyApp.Pages` becomes `MyApp.ViewModels`), then searches loaded assemblies. If two types share the name, nothing is bound.

A page that already has a `BindingContext` is left alone.

### XAML

Use this when the names don't line up:

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:inject="clr-namespace:ShellInject.Extensions;assembly=ShellInject"
    xmlns:vm="clr-namespace:MyApp.ViewModels"
    x:Class="MyApp.Pages.OrderEditorPage"
    x:DataType="vm:EditOrderViewModel"
    inject:ShellInjectPageExtensions.ViewModelType="{x:Type vm:EditOrderViewModel}">
```

`ViewModelType` always wins, including over a `BindingContext` that was set earlier. It also works on a `ContentView`.

### Registration

Map types in code at startup:

```csharp
builder.UseShellInject(options =>
{
    options.RegisterViewModel<OrderEditorPage, EditOrderViewModel>();
});
```

Registrations are checked before the naming convention and skip the assembly search. They use the same automatic binding pipeline, so they only apply while `AutoBindViewModelsByConvention` is `true`.

If you trim or publish with NativeAOT, registration or `ViewModelType` is safer than the naming convention, because the ViewModel type is referenced directly. Test your trimmed build on a device either way.

### Options

| Option | Default | Description |
|---|---|---|
| `AutoBindViewModelsByConvention` | `true` | Enables convention and registration binding. |
| `PageSuffix` | `"Page"` | Suffix removed from the page name before looking for a ViewModel. |
| `ViewModelSuffix` | `"ViewModel"` | Suffix added when looking for a ViewModel. |
| `ErrorHandler` | `null` | Called with exceptions ShellInject catches and recovers from. See [Error handling](#error-handling). |

## ViewModels

Derive from `ShellInjectViewModel`, or from `ShellInjectViewModel<T>` if the page always receives the same type. Both derive from `ObservableObject`.

| Method | When it runs |
|---|---|
| `InitializedAsync()` | Once per ViewModel instance, the first time its page appears |
| `DataReceivedAsync(parameter)` | When data is passed to this page by a push, modal, tab change, replace, or popup |
| `ReverseDataReceivedAsync(parameter)` | When a page or popup above this one returns data |
| `OnAppearing()` | Every time the page appears |
| `OnDisappearing()` | Every time the page disappears |
| `OnAppearedAsync()` | After a ShellInject navigation to this page completes |

Read navigation data in `DataReceivedAsync`, not `InitializedAsync`.

### Typed ViewModels

`ShellInjectViewModel<T>` adds overloads that take `T` instead of `object`:

```csharp
public class OrderViewModel : ShellInjectViewModel<Order>
{
    public override Task DataReceivedAsync(Order? order) { ... }
    public override Task ReverseDataReceivedAsync(Order? order) { ... }
}
```

The page type and the parameter type aren't checked against each other at compile time. If a value of the wrong type arrives, the typed method isn't called and an `InvalidCastException` is passed to `ErrorHandler`. If forward and reverse data are different types, use the non-generic base class and cast.

## Dependency injection

ShellInject resolves ViewModels from the app's service provider. Registered ViewModels use their registered lifetime. ViewModels that aren't registered are created with `ActivatorUtilities`, so their constructor dependencies are still injected.

```csharp
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddTransient<OrderViewModel>(); // optional
```

To resolve a service outside of constructor injection:

```csharp
var orders = Injector.GetRequiredService<IOrderService>();
var logger = Injector.GetService<ILogger<App>>(); // null if not registered
```

## Navigation

All methods live on the static `ShellNavigation` class. Each one takes an optional `shell` argument and uses `Shell.Current` when it's omitted. In multi-window apps, pass the window's Shell.

### Push and return

```csharp
await ShellNavigation.PushAsync<OrderPage, Order>(order);
await ShellNavigation.PushAsync<OrderPage>(parameter: order); // equivalent
```

From the pushed page, return a result to the previous page:

```csharp
await ShellNavigation.PopAsync(parameter: result);
```

The previous ViewModel receives `result` in `ReverseDataReceivedAsync`. `PopAsync` also works from a modal page. Using the system back button pops without sending data.

### Multi-page stacks

Build several pages in one call. Only the last page receives the parameter.

```csharp
await ShellNavigation.PushMultiStackAsync(
    pageTypes: [typeof(CartPage), typeof(CheckoutPage)],
    parameter: cart);
```

Return from anywhere in the stack:

```csharp
await ShellNavigation.PopToRootAsync(parameter: receipt);
await ShellNavigation.PopToAsync<CartPage>(parameter: receipt);
```

### Sending data without navigating

Deliver data to a page further down the stack while the current page stays open. The target receives it in `ReverseDataReceivedAsync`.

```csharp
await ShellNavigation.SendDataToPageAsync<OrdersPage>(data: updatedOrder);
```

### Modals

```csharp
// A single modal page
await ShellNavigation.PushModalAsync<OrderPage, Order>(order);

// A modal with its own NavigationPage
await ShellNavigation.PushModalWithNavigationAsync(page: new WizardStepOnePage(), parameter: draft);
```

`ShellNavigation.PushAsync` always pushes onto the Shell stack, even when a modal is showing. To move between pages inside a modal `NavigationPage`, use that page's `Navigation`:

```csharp
var modal = (NavigationPage)Shell.Current.Navigation.ModalStack[^1];
await modal.Navigation.PushAsync(new WizardStepTwoPage());
```

A native push like this still binds the ViewModel, but it doesn't deliver a parameter. `PopAsync` still returns data inside the modal. To close the whole modal stack and send data to the page underneath, use:

```csharp
await ShellNavigation.PopModalStackAsync(data: draft);
```

### Tabs

```csharp
await ShellNavigation.ChangeTabAsync(tabIndex: 1, parameter: filter);
await ShellNavigation.ChangeTabAsync<OrdersPage>(parameter: filter);
```

The generic version selects the tab whose page is already an `OrdersPage`. It doesn't create pages from `ContentTemplate` to look for a match, so declare the page directly in `AppShell.xaml` if you select it by type. If no tab matches, it falls back to `tabIndex`. By default the current stack is popped to root first. Pass `popToRootFirst: false` to keep it.

### Flyout items

Switch to a page that's already part of the Shell hierarchy:

```csharp
await ShellNavigation.ReplaceAsync<ReportsPage>(parameter: dateRange);
```

The `ShellContent` route must match the page class name:

```xml
<FlyoutItem Title="Reports">
    <ShellContent Route="ReportsPage" ContentTemplate="{DataTemplate pages:ReportsPage}" />
</FlyoutItem>
```

The destination receives the data in `DataReceivedAsync`, even when you're switching back to a page you came from.

### Popups

Popups need `UseMauiCommunityToolkit()` at startup. They're bound to ViewModels the same way pages are.

```csharp
await ShellNavigation.ShowPopupAsync<ConfirmPopup>(data: order);
```

From the popup's ViewModel:

```csharp
await ShellNavigation.DismissPopupAsync<ConfirmPopup>(data: true); // result goes to the page's ReverseDataReceivedAsync
await ShellNavigation.DismissPopupAsync<ConfirmPopup>();           // closes without sending anything
```

In multi-window apps, pass the same `shell` to both calls.

### API summary

| Method | Delivers data to |
|---|---|
| `PushAsync<TPage>` / `PushAsync<TPage, TParameter>` | New page, `DataReceivedAsync` |
| `PushModalAsync<TPage>` / `PushModalAsync<TPage, TParameter>` | Modal page, `DataReceivedAsync` |
| `PushModalWithNavigationAsync` | Modal root page, `DataReceivedAsync` |
| `PushMultiStackAsync` | Last page in the stack, `DataReceivedAsync` |
| `PopAsync` | Previous page, `ReverseDataReceivedAsync` |
| `PopToAsync<TPage>` | Matching page on the Shell stack, `ReverseDataReceivedAsync` |
| `PopToRootAsync` | Root page, `ReverseDataReceivedAsync` |
| `PopModalStackAsync` | Page under the modal, `ReverseDataReceivedAsync` |
| `SendDataToPageAsync<TPage>` | Matching page on the Shell stack, `ReverseDataReceivedAsync` |
| `ChangeTabAsync` / `ChangeTabAsync<TPage>` | Selected tab, `DataReceivedAsync` |
| `ReplaceAsync<TPage>` | Destination page, `DataReceivedAsync` |
| `ShowPopupAsync<TPopup>` | Popup, `DataReceivedAsync` |
| `DismissPopupAsync<TPopup>` | Current page, `ReverseDataReceivedAsync` |

## Error handling

Navigation methods throw as usual, so wrap calls in `try`/`catch` where you need to. Some failures happen outside a call you can catch, such as a ViewModel constructor throwing during binding or an exception in a lifecycle method. ShellInject catches those so the app keeps running. To see them:

```csharp
builder.UseShellInject(options =>
{
    options.ErrorHandler = ex => System.Diagnostics.Debug.WriteLine(ex);
});
```

A page with no matching ViewModel isn't treated as an error.

## Upgrading from 10.0

10.1 doesn't break existing code. Package validation checks the public API against 10.0.3.

**Shell extension methods are obsolete.** Calls like `Shell.Current.PushAsync<OrderPage>(order)` still compile and behave the same, but now produce a warning. Replace them with the matching `ShellNavigation` method:

```csharp
// Before
await Shell.Current.PushAsync<OrderPage>(order);

// After
await ShellNavigation.PushAsync<OrderPage>(parameter: order);
```

**`OnDisAppearing` is now `OnDisappearing`.** Existing `OnDisAppearing` overrides still work and don't produce warnings. Override one or the other, not both.

New in 10.1:

- `ShellInjectViewModel<T>` with typed data methods
- `PushAsync<TPage, TParameter>` and `PushModalAsync<TPage, TParameter>`
- `options.RegisterViewModel<TView, TViewModel>()`
- `options.ErrorHandler`
- `ChangeTabAsync<TPage>`
- Cached convention lookups
- Popup tracking per Shell, for multi-window apps
- `SendDataToPageAsync` matches the exact page type first, then falls back to the class name as before

## Troubleshooting

**"An error occurred trying to navigate with Shell navigation"**
`Shell.Current` was null. Make sure the window's root page is a `Shell`, or pass `shell:` explicitly.

**The ViewModel isn't bound**
Check that the names match (`OrderPage` to `OrderViewModel` or `OrderPageViewModel`) and that only one type has that name. Also check that nothing sets `BindingContext` first, including XAML such as `<ContentPage.BindingContext>`. To see why a ViewModel couldn't be created, set `ErrorHandler`.

**`DataReceivedAsync` isn't called**
The ViewModel must derive from `ShellInjectViewModel` or implement `IShellInjectShellViewModel`. For typed ViewModels, check `ErrorHandler` for a type mismatch.

**`ChangeTabAsync<TPage>` selects the wrong tab**
The target page probably isn't created yet. Declare it directly inside `ShellContent` instead of using `ContentTemplate`.

**`PopToRootAsync` doesn't close my modal**
It only works on the Shell stack. Use `PopModalStackAsync` for modals.

## Sample app

The [`Sample`](https://github.com/scryptfyre/ShellInject/tree/main/Sample) folder contains a MAUI app that walks through every workflow above. It includes an in-app setup guide and a live log of each navigation call and lifecycle callback. See the [sample README](https://github.com/scryptfyre/ShellInject/blob/main/Sample/README.md) for details.

## Building from source

```bash
dotnet build ShellInject/ShellInject.csproj -c Release
dotnet test ShellInjectTests/ShellInjectTests.csproj
dotnet build Sample/Sample.csproj -f net10.0-android
```

## License

[Apache 2.0](https://github.com/scryptfyre/ShellInject/blob/main/LICENSE)
