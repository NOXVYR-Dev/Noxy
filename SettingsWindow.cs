using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace Noxy;

public class SettingsWindow : Window
{
    private readonly UpdateService updates;
    private readonly TextBlock updateStatus = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) };
    private readonly WpfButton installUpdate = new() { Content = "Jetzt aktualisieren", Height = 34, IsEnabled = false, Padding = new Thickness(12, 0, 12, 0) };
    private readonly WpfButton checkUpdates = new() { Content = "Jetzt prüfen", Height = 34, Margin = new Thickness(8, 0, 0, 0) };

    public SettingsWindow(PetSettings settings, UpdateService updateService)
    {
        updates = updateService;
        Title = "Noxy | Einstellungen";
        Width = 440;
        Height = 680;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = System.Windows.Media.Brushes.Black;
        Foreground = System.Windows.Media.Brushes.White;

        var panel = new StackPanel { Margin = new Thickness(20) };
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        panel.Children.Add(new TextBlock { Text = $"Noxy {updates.CurrentVersion}", FontSize = 19, Margin = new Thickness(0, 0, 0, 15) });

        panel.Children.Add(new TextBlock { Text = "Verhalten", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
        var movement = new WpfComboBox { Height = 32, Margin = new Thickness(0, 0, 0, 12) };
        movement.Items.Add("Herumlaufen");
        movement.Items.Add("Zappelig");
        movement.Items.Add("Statisch");
        movement.Items.Add("Schlafmodus");
        movement.SelectedIndex = settings.MovementMode switch { "zippy" => 1, "static" => 2, "sleep" => 3, _ => 0 };
        panel.Children.Add(movement);
        var speech = AddCheck(panel, "Freche Sprüche aktiv", settings.EnableSpeech);
        var obs = AddCheck(panel, "Ruhemodus bei OBS", settings.QuietWhenObs);
        var full = AddCheck(panel, "Ruhemodus bei Vollbild", settings.QuietWhenFullscreen);
        var sensors = AddCheck(panel, "RAM-Warnungen aktiv", settings.EnableSensors);
        var autostart = AddCheck(panel, "Beim Windows-Anmelden automatisch starten", WindowsStartup.IsEnabled());

        panel.Children.Add(new TextBlock { Text = "Sprüche alle (Sekunden, mindestens 15):", Margin = new Thickness(0, 6, 0, 0) });
        var interval = new WpfTextBox { Text = settings.IntervalSeconds.ToString(), Margin = new Thickness(0, 5, 0, 12) };
        panel.Children.Add(interval);

        var save = new WpfButton { Content = "Speichern", Height = 35 };
        panel.Children.Add(save);
        save.Click += (_, _) =>
        {
            settings.MovementMode = movement.SelectedIndex switch { 1 => "zippy", 2 => "static", 3 => "sleep", _ => "walk" };
            settings.EnableWalking = settings.MovementMode is "walk" or "zippy";
            settings.EnableSpeech = speech.IsChecked == true;
            settings.QuietWhenObs = obs.IsChecked == true;
            settings.QuietWhenFullscreen = full.IsChecked == true;
            settings.EnableSensors = sensors.IsChecked == true;
            if (int.TryParse(interval.Text, out var seconds)) settings.IntervalSeconds = Math.Clamp(seconds, 15, 3600);
            try
            {
                WindowsStartup.SetEnabled(autostart.IsChecked == true);
                settings.Save();
                updateStatus.Text = "Einstellungen gespeichert.";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(this, $"Die Autostart-Einstellung konnte nicht gespeichert werden.\n\n{ex.Message}", "Noxy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        panel.Children.Add(new Separator { Margin = new Thickness(0, 18, 0, 12) });
        panel.Children.Add(new TextBlock { Text = "Updates von GitHub", FontSize = 16, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(updateStatus);
        panel.Children.Add(new TextBlock { Text = "Änderungen der neuesten Version", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 4, 0, 6) });
        var changelog = new WpfTextBox
        {
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MinHeight = 74,
            MaxHeight = 130,
            Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 30, 36)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(78, 70, 82)),
            Padding = new Thickness(8),
        };
        panel.Children.Add(changelog);
        var updateButtons = new StackPanel { Orientation = WpfOrientation.Horizontal };
        updateButtons.Children.Add(installUpdate);
        updateButtons.Children.Add(checkUpdates);
        panel.Children.Add(updateButtons);

        updates.Changed += RefreshUpdateState;
        checkUpdates.Click += async (_, _) => await updates.CheckAsync();
        installUpdate.Click += async (_, _) => await updates.InstallUpdateAsync();
        void RefreshChangelog() => changelog.Text = updates.ChangeLog;
        updates.Changed += RefreshChangelog;
        Closed += (_, _) =>
        {
            updates.Changed -= RefreshUpdateState;
            updates.Changed -= RefreshChangelog;
        };
        RefreshUpdateState();
        RefreshChangelog();
    }

    private static WpfCheckBox AddCheck(System.Windows.Controls.Panel panel, string label, bool enabled)
    {
        var check = new WpfCheckBox
        {
            Content = label,
            Foreground = System.Windows.Media.Brushes.White,
            IsChecked = enabled,
            Margin = new Thickness(0, 0, 0, 10),
        };
        panel.Children.Add(check);
        return check;
    }

    private void RefreshUpdateState()
    {
        updateStatus.Text = updates.Status;
        installUpdate.IsEnabled = updates.UpdateAvailable && !updates.IsBusy;
        installUpdate.Background = updates.UpdateAvailable ? System.Windows.Media.Brushes.ForestGreen : null;
        installUpdate.Foreground = updates.UpdateAvailable ? System.Windows.Media.Brushes.White : System.Windows.SystemColors.ControlTextBrush;
        checkUpdates.IsEnabled = !updates.IsBusy;
    }
}

