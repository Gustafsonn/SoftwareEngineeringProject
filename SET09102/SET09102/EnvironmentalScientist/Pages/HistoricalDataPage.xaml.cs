using SET09102.Models;
using Microsoft.Maui.Controls;
using System;

namespace SET09102.EnvironmentalScientist.Pages
{
    public partial class HistoricalDataPage : ContentPage
    {
        private readonly HistoricalDataViewModel _viewModel;

        public HistoricalDataPage(HistoricalDataViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void OnDataTypeChanged(object sender, EventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }
    }
}