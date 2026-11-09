using System.Windows;
using System.Windows.Controls;

namespace WarCalc.Wpf;

public partial class UnitsWindow : Window
{
    public UnitsWindow()
    {
        InitializeComponent();
        Form.DataContext = new Unit();
    }

    async void Window_Loaded(object s, RoutedEventArgs e)
    {
        await Api.Safe(async () =>
        {
            var factions = await Api.Get<List<Faction>>("api/factions");
            FormFaction.ItemsSource = factions;
            FilterFaction.ItemsSource = factions.Prepend(new Faction { Id = 0, Name = "(all factions)" }).ToList();
            FilterFaction.SelectedIndex = 0;
        });
        await Reload();
    }

    // Uses the custom GET api/units/search endpoint.
    async Task Reload() => await Api.Safe(async () =>
    {
        var url = $"api/units/search?name={Uri.EscapeDataString(SearchBox.Text)}&factionId={FilterFaction.SelectedValue ?? 0}";
        UnitGrid.ItemsSource = await Api.Get<List<Unit>>(url);
        Form.DataContext = new Unit();
    });

    async void Search_Click(object s, RoutedEventArgs e) => await Reload();

    void UnitGrid_SelectionChanged(object s, SelectionChangedEventArgs e)
    {
        if (UnitGrid.SelectedItem is Unit u) Form.DataContext = Api.Clone(u);
    }

    void New_Click(object s, RoutedEventArgs e)
    {
        UnitGrid.SelectedItem = null;
        Form.DataContext = new Unit { FactionId = FilterFaction.SelectedValue is int id ? id : 0 };
    }

    async void Save_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        var u = (Unit)Form.DataContext;
        if (u.Id == 0) await Api.Post<Unit>("api/units", u);
        else await Api.Put($"api/units/{u.Id}", u);
        await Reload();
    });

    async void Delete_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        var u = (Unit)Form.DataContext;
        if (u.Id == 0 || !Api.Confirm($"Delete \"{u.Name}\"?")) return;
        await Api.Delete($"api/units/{u.Id}");
        await Reload();
    });

    void Weapons_Click(object s, RoutedEventArgs e)
    {
        if (Form.DataContext is Unit { Id: > 0 } u) new WeaponsWindow(u) { Owner = this }.Show();
        else MessageBox.Show("Select or save a unit first.");
    }
}
