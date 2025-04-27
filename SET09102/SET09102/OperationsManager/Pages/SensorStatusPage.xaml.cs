using SET09102.Models;
using SET09102.Services;
using System.Windows.Input;

namespace SET09102.OperationsManager.Pages;

public partial class SensorStatusPage : ContentPage
{
    private readonly SensorStatusViewModel _viewModel;

    public SensorStatusPage(SensorStatusViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void OnShowIssuesOnlyToggled(object sender, ToggledEventArgs e)
    {
        if (_viewModel.ToggleIssuesCommand.CanExecute(null))
        {
            _viewModel.ToggleIssuesCommand.Execute(null);
        }
    }
}