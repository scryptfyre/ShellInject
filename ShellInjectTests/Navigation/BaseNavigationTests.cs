using Moq;
using ShellInject.Interfaces;

namespace ShellInjectTests.Navigation;

public abstract class BaseNavigationTests
{
    protected class TestPage : ContentPage;
    protected Type TestPageType { get; } = typeof(TestPage);
    protected Shell TestShell { get; private set; } = new();
    protected object TestNavigationParameter { get; } = new { Value = "my test data" };
    protected TestPage MockPage { get; private set; } = new();
    protected Mock<IShellInjectShellViewModel> MockShellViewModel { get; } = new();
    protected ShellNavigatedEventArgs NavigatedEventArgsPush = new("/previousPage", "/currentPage", ShellNavigationSource.Push);

    protected void SetupShell_Page_And_BindingContext()
    {
        MockPage = new TestPage
        {
            BindingContext = MockShellViewModel.Object
        };

        var testShell = new Shell();
        var shellContent = new ShellContent { Content = MockPage };
        var shellSection = new ShellSection();
        shellSection.Items.Add(shellContent);
        shellSection.CurrentItem = shellContent;
        var shellItem = new ShellItem();
        shellItem.Items.Add(shellSection);
        shellItem.CurrentItem = shellSection;
        testShell.Items.Add(shellItem);
        testShell.CurrentItem = shellItem;
        TestShell = testShell;
    }
}
