using Moq;
using ShellInject.Interfaces;
using ShellInject.Navigation;

namespace ShellInjectTests.Navigation;

public class ChangeTabTests
{
    private readonly object _parameter = new { Value = "tab data" };

    [Fact]
    public async Task ChangeTabAsync_WhenTargetTabHasContent_ShouldSendDataToTargetTabOnly()
    {
        var firstViewModel = new Mock<IShellInjectShellViewModel>();
        var secondViewModel = new Mock<IShellInjectShellViewModel>();
        var firstPage = new ContentPage { BindingContext = firstViewModel.Object };
        var secondPage = new ContentPage { BindingContext = secondViewModel.Object };
        var shell = CreateTabbedShell(
            new ShellContent { Content = firstPage },
            new ShellContent { Content = secondPage });
        var nav = new ShellInjectNavigation();

        await nav.ChangeTabAsync(shell, 1, _parameter, popToRootFirst: false);

        firstViewModel.Verify(vm => vm.DataReceivedAsync(It.IsAny<object?>()), Times.Never);
        secondViewModel.Verify(vm => vm.DataReceivedAsync(_parameter), Times.Once);
    }

    [Fact]
    public async Task ChangeTabAsync_WhenTargetTabUsesContentTemplate_ShouldMaterializeAndSendDataToTargetTab()
    {
        var firstViewModel = new Mock<IShellInjectShellViewModel>();
        var secondViewModel = new Mock<IShellInjectShellViewModel>();
        var firstPage = new ContentPage { BindingContext = firstViewModel.Object };
        var targetShellContent = new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new ContentPage { BindingContext = secondViewModel.Object })
        };
        var shell = CreateTabbedShell(new ShellContent { Content = firstPage }, targetShellContent);
        var nav = new ShellInjectNavigation();

        await nav.ChangeTabAsync(shell, 1, _parameter, popToRootFirst: false);

        firstViewModel.Verify(vm => vm.DataReceivedAsync(It.IsAny<object?>()), Times.Never);
        secondViewModel.Verify(vm => vm.DataReceivedAsync(_parameter), Times.Once);
        Assert.IsType<ContentPage>(targetShellContent.Content);
    }

    [Fact]
    public async Task ChangeTabAsync_WhenTargetTabIsInDifferentShellItem_ShouldSelectShellItemAndSendData()
    {
        var mainViewModel = new Mock<IShellInjectShellViewModel>();
        var firstTabViewModel = new Mock<IShellInjectShellViewModel>();
        var secondTabViewModel = new Mock<IShellInjectShellViewModel>();
        var mainContent = new ShellContent { Content = new ContentPage { BindingContext = mainViewModel.Object } };
        var firstTabContent = new ShellContent { Content = new ContentPage { BindingContext = firstTabViewModel.Object } };
        var secondTabContent = new ShellContent { Content = new ContentPage { BindingContext = secondTabViewModel.Object } };
        var mainSection = new ShellSection { CurrentItem = mainContent };
        mainSection.Items.Add(mainContent);
        var tabSection = new ShellSection { CurrentItem = firstTabContent };
        tabSection.Items.Add(firstTabContent);
        tabSection.Items.Add(secondTabContent);
        var mainItem = new ShellItem { CurrentItem = mainSection };
        mainItem.Items.Add(mainSection);
        var tabItem = new ShellItem { CurrentItem = tabSection };
        tabItem.Items.Add(tabSection);
        var shell = new Shell { CurrentItem = mainItem };
        shell.Items.Add(mainItem);
        shell.Items.Add(tabItem);
        var nav = new ShellInjectNavigation();

        await nav.ChangeTabAsync(shell, 1, _parameter, popToRootFirst: false);

        Assert.Same(tabItem, shell.CurrentItem);
        Assert.Same(tabSection, tabItem.CurrentItem);
        Assert.Same(secondTabContent, tabSection.CurrentItem);
        mainViewModel.Verify(vm => vm.DataReceivedAsync(It.IsAny<object?>()), Times.Never);
        firstTabViewModel.Verify(vm => vm.DataReceivedAsync(It.IsAny<object?>()), Times.Never);
        secondTabViewModel.Verify(vm => vm.DataReceivedAsync(_parameter), Times.Once);
    }

    private static Shell CreateTabbedShell(ShellContent firstShellContent, ShellContent secondShellContent)
    {
        var shellSection = new ShellSection();
        shellSection.Items.Add(firstShellContent);
        shellSection.Items.Add(secondShellContent);
        shellSection.CurrentItem = firstShellContent;

        var shellItem = new ShellItem();
        shellItem.Items.Add(shellSection);
        shellItem.CurrentItem = shellSection;

        var shell = new Shell();
        shell.Items.Add(shellItem);
        shell.CurrentItem = shellItem;

        return shell;
    }
}
