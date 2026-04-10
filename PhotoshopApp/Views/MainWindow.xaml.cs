using PhotoshopApp.UI.ViewModels;

namespace PhotoshopApp;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}