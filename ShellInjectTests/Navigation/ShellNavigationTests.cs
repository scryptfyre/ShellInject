using ShellInject;
using ShellInject.Constants;

namespace ShellInjectTests.Navigation;

public class ShellNavigationTests
{
    private class TestPage : ContentPage;

    [Fact]
    public async Task PushAsync_WhenShellIsUnavailable_ShouldThrow()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => ShellNavigation.PushAsync<TestPage>());
        Assert.Equal(ShellInjectConstants.ShellNotFoundText, exception.Message);
    }

    [Fact]
    public async Task FacadeMethods_WhenShellIsUnavailable_ShouldThrowConsistentException()
    {
        var assertions = new Func<Task>[]
        {
            () => ShellNavigation.ReplaceAsync<TestPage>(),
            () => ShellNavigation.PopAsync(),
            () => ShellNavigation.PopModalStackAsync(),
            () => ShellNavigation.PopToAsync<TestPage>(),
            () => ShellNavigation.PopToRootAsync(),
            () => ShellNavigation.ChangeTabAsync(),
            () => ShellNavigation.PushMultiStackAsync(),
            () => ShellNavigation.PushModalWithNavigationAsync(page: new TestPage()),
            () => ShellNavigation.PushModalAsync<TestPage>(),
            () => ShellNavigation.SendDataToPageAsync<TestPage>(),
            () => ShellNavigation.ShowPopupAsync<TestPopup>(),
            () => ShellNavigation.DismissPopupAsync<TestPopup>()
        };

        foreach (var assertion in assertions)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(assertion);
            Assert.Equal(ShellInjectConstants.ShellNotFoundText, exception.Message);
        }
    }

    [Fact]
    public async Task FacadeMethod_WhenExplicitShellIsProvided_ShouldResolveShell()
    {
        await ShellNavigation.DismissPopupAsync<TestPopup>(new Shell());
    }

    [Fact]
    public async Task ObsoleteExtensionMethods_WhenShellIsNull_ShouldForwardToShellNavigation()
    {
        Shell shell = null!;
#pragma warning disable CS0618 // Intentional coverage for obsolete compatibility wrappers.
        var assertions = new Func<Task>[]
        {
            () => shell.PushAsync<TestPage>(),
            () => shell.ReplaceAsync<TestPage>(),
            () => shell.PopAsync(),
            () => shell.PopModalStackAsync(),
            () => shell.PopToAsync<TestPage>(),
            () => shell.PopToRootAsync(),
            () => shell.ChangeTabAsync(0),
            () => shell.PushMultiStackAsync([]),
            () => shell.PushModalWithNavigationAsync(new TestPage()),
            () => shell.PushModalAsync<TestPage>(),
            () => shell.SendDataToPageAsync<TestPage>(),
            () => shell.ShowPopupAsync<TestPopup>(),
            () => shell.DismissPopupAsync<TestPopup>()
        };
#pragma warning restore CS0618

        foreach (var assertion in assertions)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(assertion);
            Assert.Equal(ShellInjectConstants.ShellNotFoundText, exception.Message);
        }
    }

    private class TestPopup : CommunityToolkit.Maui.Views.Popup;
}
