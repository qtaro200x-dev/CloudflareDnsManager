using System.Windows;
using System.Windows.Controls;
using CloudflareDnsManager.Models;
using CloudflareDnsManager.ViewModels;


namespace CloudflareDnsManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.Row.Item is DnsRecord record)
        {
            record.IsModified = true;
        }
    }
}

