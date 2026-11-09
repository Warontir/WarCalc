using System.Windows;

namespace WarCalc.Wpf;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    void Army_Click(object s, RoutedEventArgs e) => new ArmyBuilderWindow { Owner = this }.Show();
    void Units_Click(object s, RoutedEventArgs e) => new UnitsWindow { Owner = this }.Show();
    void Factions_Click(object s, RoutedEventArgs e) => new FactionsWindow { Owner = this }.Show();
}
