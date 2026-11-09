using System.Windows;
using System.Windows.Controls;

namespace WarCalc.Wpf;

public partial class FactionsWindow : Window
{
    public FactionsWindow()
    {
        InitializeComponent();
        Form.DataContext = new Faction();
    }

    async void Window_Loaded(object s, RoutedEventArgs e) => await Reload();

    async Task Reload() => await Api.Safe(async () =>
    {
        FactionGrid.ItemsSource = await Api.Get<List<Faction>>("api/factions");
        Form.DataContext = new Faction();
    });

    void FactionGrid_SelectionChanged(object s, SelectionChangedEventArgs e)
    {
        if (FactionGrid.SelectedItem is Faction f) Form.DataContext = Api.Clone(f);
    }

    void New_Click(object s, RoutedEventArgs e)
    {
        FactionGrid.SelectedItem = null;
        Form.DataContext = new Faction();
    }

    async void Save_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        var f = (Faction)Form.DataContext;
        if (f.Id == 0) await Api.Post<Faction>("api/factions", f);
        else await Api.Put($"api/factions/{f.Id}", f);
        await Reload();
    });

    async void Delete_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        var f = (Faction)Form.DataContext;
        if (f.Id == 0 || !Api.Confirm($"Delete \"{f.Name}\" together with all its units, weapons and armies?")) return;
        await Api.Delete($"api/factions/{f.Id}");
        await Reload();
    });
}
