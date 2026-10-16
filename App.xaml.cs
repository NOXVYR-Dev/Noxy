using System.Windows;
namespace Noxy;
public partial class App : System.Windows.Application {
    private bool ownsInstanceSlot;
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        var settings=PetSettings.Load();
        if(!ProcessInstanceManager.TryAcquire(settings.MaxInstances,out var slot)) { Shutdown(); return; }
        ownsInstanceSlot=true;
        var window=new MainWindow(settings,slot);
        MainWindow=window;
        window.Show();
    }
    protected override void OnExit(ExitEventArgs e) { if(ownsInstanceSlot) ProcessInstanceManager.Release(); base.OnExit(e); }
}

