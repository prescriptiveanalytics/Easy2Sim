using System.Windows;

namespace Easy2SimVisualizationWpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowVm();
    }
}
