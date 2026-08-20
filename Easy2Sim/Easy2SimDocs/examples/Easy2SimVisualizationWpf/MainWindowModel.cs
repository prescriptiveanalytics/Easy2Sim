using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveCharts;

namespace Easy2SimVisualizationWpf;

/// <summary>
/// Holds the data that is shown in the view.
/// </summary>
public class MainWindowModel : INotifyPropertyChanged
{
    private ChartValues<double> _sineValues = new ChartValues<double>();

    /// <summary>
    /// All data points that are shown in the chart.
    /// ChartValues implements INotifyCollectionChanged, so the chart
    /// redraws automatically when values are added or removed.
    /// </summary>
    public ChartValues<double> SineValues
    {
        get => _sineValues;
        set
        {
            _sineValues = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
