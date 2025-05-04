using System.Diagnostics;

namespace SET09102.EnvironmentalScientist.Pages;

public partial class DisplayThresholdAlerts : ContentPage
{
	public DisplayThresholdAlerts()
	{
		try
		{
			InitializeComponent();
		}
		catch (Exception ex)
		{
            Debug.WriteLine("Error in DisplayThresholdAlerts: " + ex.Message);
        }
    }
}