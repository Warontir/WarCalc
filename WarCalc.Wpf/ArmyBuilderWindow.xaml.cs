using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WarCalc.Wpf;

public partial class ArmyBuilderWindow : Window
{
    public ArmyBuilderWindow()
    {
        InitializeComponent();
        Form.DataContext = new Army();
    }

    async void Window_Loaded(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        FactionBox.ItemsSource = await Api.Get<List<Faction>>("api/factions");
        await ReloadArmies();
    });

    async Task ReloadArmies(int? selectId = null)
    {
        var armies = await Api.Get<List<Army>>("api/armies");
        ArmyList.ItemsSource = armies;
        ArmyList.SelectedItem = armies.FirstOrDefault(a => a.Id == selectId);
    }

    async void ArmyList_SelectionChanged(object s, SelectionChangedEventArgs e) => await Api.Safe(async () =>
    {
        if (ArmyList.SelectedItem is not Army a) return;
        Form.DataContext = Api.Clone(a);
        AvailableGrid.ItemsSource = await Api.Get<List<Unit>>($"api/factions/{a.FactionId}/units");
        ShowSummary(await Api.Get<ArmySummary>($"api/armies/{a.Id}/summary"));
    });

    void ShowSummary(ArmySummary s)
    {
        LineGrid.ItemsSource = s.Lines;
        SummaryText.Text = s.IsValid
            ? $"{s.Name} ({s.Faction}): {s.TotalPoints} / {s.PointsLimit} pts - {s.RemainingPoints} pts left"
            : $"{s.Name} ({s.Faction}): {s.TotalPoints} / {s.PointsLimit} pts - OVER by {-s.RemainingPoints} pts!";
        SummaryText.Foreground = s.IsValid ? Brushes.DarkGreen : Brushes.Firebrick;
    }

    void ClearDetails()
    {
        LineGrid.ItemsSource = null;
        AvailableGrid.ItemsSource = null;
        SummaryText.Text = "Select or create an army";
        SummaryText.Foreground = Brushes.Black;
    }

    // ---- army CRUD ----
    void NewArmy_Click(object s, RoutedEventArgs e)
    {
        ArmyList.SelectedItem = null;
        ClearDetails();
        Form.DataContext = new Army();
    }

    async void SaveArmy_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        var a = (Army)Form.DataContext;
        var id = a.Id;
        if (id == 0) id = (await Api.Post<Army>("api/armies", a)).Id;
        else await Api.Put($"api/armies/{a.Id}", a);
        await ReloadArmies(id);
    });

    async void DeleteArmy_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        if (ArmyList.SelectedItem is not Army a || !Api.Confirm($"Delete army \"{a.Name}\"?")) return;
        await Api.Delete($"api/armies/{a.Id}");
        ClearDetails();
        Form.DataContext = new Army();
        await ReloadArmies();
    });

    // ---- custom endpoints: POST/DELETE api/armies/{id}/units ----
    async void Add_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        if (ArmyList.SelectedItem is not Army a || AvailableGrid.SelectedItem is not Unit u)
        {
            MessageBox.Show("Select an army and a unit from the available list.");
            return;
        }
        if (!int.TryParse(QtyBox.Text, out var qty) || qty < 1) qty = 1;
        ShowSummary(await Api.Post<ArmySummary>($"api/armies/{a.Id}/units", new { unitId = u.Id, quantity = qty }));
    });

    async void Remove_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        if (ArmyList.SelectedItem is not Army a || LineGrid.SelectedItem is not ArmyLine line) return;
        ShowSummary(await Api.Delete<ArmySummary>($"api/armies/{a.Id}/units/{line.UnitId}"));
    });
}
