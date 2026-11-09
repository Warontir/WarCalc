using System.Windows;
using System.Windows.Controls;

namespace WarCalc.Wpf;

public partial class WeaponsWindow : Window
{
    readonly Unit _unit;

    public WeaponsWindow(Unit unit)
    {
        InitializeComponent();
        _unit = unit;
        Title = $"Weapons - {unit.Name}";
        Form.DataContext = new Weapon();
    }

    async void Window_Loaded(object s, RoutedEventArgs e)
    {
        await Api.Safe(async () => TargetBox.ItemsSource = await Api.Get<List<Unit>>("api/units"));
        await Reload();
    }

    async Task Reload() => await Api.Safe(async () =>
    {
        WeaponGrid.ItemsSource = await Api.Get<List<Weapon>>($"api/weapons?unitId={_unit.Id}");
        Form.DataContext = new Weapon();
    });

    void WeaponGrid_SelectionChanged(object s, SelectionChangedEventArgs e)
    {
        if (WeaponGrid.SelectedItem is Weapon w) Form.DataContext = Api.Clone(w);
    }

    void New_Click(object s, RoutedEventArgs e)
    {
        WeaponGrid.SelectedItem = null;
        Form.DataContext = new Weapon();
    }

    async void Save_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        var w = (Weapon)Form.DataContext;
        w.UnitId = _unit.Id;
        if (w.Id == 0) await Api.Post<Weapon>("api/weapons", w);
        else await Api.Put($"api/weapons/{w.Id}", w);
        await Reload();
    });

    async void Delete_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        var w = (Weapon)Form.DataContext;
        if (w.Id == 0 || !Api.Confirm($"Delete \"{w.Name}\"?")) return;
        await Api.Delete($"api/weapons/{w.Id}");
        await Reload();
    });

    // Uses the custom GET api/weapons/{id}/damage/{targetUnitId} endpoint.
    async void Calc_Click(object s, RoutedEventArgs e) => await Api.Safe(async () =>
    {
        if (WeaponGrid.SelectedItem is not Weapon w || TargetBox.SelectedValue is not int targetId)
        {
            MessageBox.Show("Select a weapon in the list and a target unit.");
            return;
        }
        if (!int.TryParse(AttackersBox.Text, out var attackers) || attackers < 1) attackers = 1;

        var r = await Api.Get<DamageResult>($"api/weapons/{w.Id}/damage/{targetId}?attackers={attackers}");
        ResultText.Text =
            $"{r.Attackers} x {r.Weapon}  ->  {r.Target}\n" +
            $"Hit {r.HitChance:P0}   Wound {r.WoundChance:P0}   Save fails {r.FailedSaveChance:P0}\n" +
            $"Unsaved wounds : {r.ExpectedUnsavedWounds:0.00}\n" +
            $"Damage dealt   : {r.ExpectedDamage:0.00}\n" +
            $"Models slain   : {r.ExpectedModelsSlain:0.00}";
    });
}
