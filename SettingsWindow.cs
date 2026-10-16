using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace Noxy;

public class SettingsWindow : Window
{
    private readonly UpdateService updates;
    private readonly TextBlock updateStatus = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) };
    private readonly WpfButton installUpdate = new() { Content = "Jetzt aktualisieren", Height = 34, IsEnabled = false };
    private readonly WpfButton checkUpdates = new() { Content = "Jetzt prüfen", Height = 34, Margin = new Thickness(8, 0, 0, 0) };

    public SettingsWindow(PetSettings settings, UpdateService updateService)
    {
        updates = updateService;
        Title = "Noxy | Einstellungen";
        Width = 410;
        Height = 570;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = System.Windows.Media.Brushes.Black;
        Foreground = System.Windows.Media.Brushes.White;

        var panel = new StackPanel { Margin = new Thickness(20) };
        Content = panel;
        panel.Children.Add(new TextBlock { Text = $"Noxy {updates.CurrentVersion}", FontSize = 19, Margin = new Thickness(0, 0, 0, 15) });

        var walking = AddCheck(panel, "Automatisch herumlaufen", settings.EnableWalking);
        var speech = AddCheck(panel, "Freche Sprüche aktiv", settings.EnableSpeech);
        var obs = AddCheck(panel, "Ruhemodus bei OBS", settings.QuietWhenObs);
        var full = AddCheck(panel, "Ruhemodus bei Vollbild", settings.QuietWhenFullscreen);
        var sensors = AddCheck(panel, "RAM-Warnungen aktiv", settings.EnableSensors);

        panel.Children.Add(new TextBlock { Text = "Sprüche alle (Sekunden, mindestens 15):", Margin = new Thickness(0, 6, 0, 0) });
        var interval = new WpfTextBox { Text = settings.IntervalSeconds.ToString(), Margin = new Thickness(0, 5, 0, 12) };
        panel.Children.Add(interval);

        var save = new WpfButton { Content = "Speichern", Height = 35 };
        panel.Children.Add(save);
        save.Click += (_, _) =>
        {
            settings.EnableWalking = walking.IsChecked == true;
            settings.EnableSpeech = speech.IsChecked == true;
            settings.QuietWhenObs = obs.IsChecked == true;
            settings.QuietWhenFullscreen = full.IsChecked == true;
            settings.EnableSensors = sensors.IsChecked == true;
            if (int.TryParse(interval.Text, out var seconds)) settings.IntervalSeconds = Math.Clamp(seconds, 15, 3600);
            settings.Save();
        };

        panel.Children.Add(new Separator { Margin = new Thickness(0, 18, 0, 12) });
        panel.Children.Add(new TextBlock { Text = "Updates von GitHub", FontSize = 16, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(updateStatus);
        var updateButtons = new StackPanel { Orientation = WpfOrientation.Horizontal };
        updateButtons.Children.Add(installUpdate);
        updateButtons.Children.Add(checkUpdates);
        panel.Children.Add(updateButtons);

        updates.Changed += RefreshUpdateState;
        checkUpdates.Click += async (_, _) => await updates.CheckAsync();
        installUpdate.Click += async (_, _) => await updates.InstallUpdateAsync();
        Closed += (_, _) => updates.Changed -= RefreshUpdateState;
        RefreshUpdateState();
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
        checkUpdates.IsEnabled = !updates.IsBusy;
    }
}

