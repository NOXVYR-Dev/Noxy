using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace Noxy;
public partial class MainWindow : Window {
 readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromSeconds(1)};
 readonly DispatcherTimer movementTimer=new(){Interval=TimeSpan.FromMilliseconds(33)};
 readonly Random random=new();
 readonly UpdateService updates=new();
 double velocity=1.25, phase=0; int motionTicks=0, idleTicks=40; bool dragging=false; readonly PetSettings settings=PetSettings.Load();
 int seconds=0, bubbleTime=0; bool lastFallout=false,lastWow=false;
 readonly string[] phrases={"Dein Desktop sieht aus, als hätte ein Supermutant aufgeräumt.","Ich habe heute nichts Produktives gemacht. Wir haben so viel gemeinsam.","Du klickst ziemlich selbstbewusst für jemanden ohne Plan.","Fallout im Herzen, ButzGatz im Blut. Und Chaos im Arbeitsspeicher.","Dein PC arbeitet. Ich überwache das fachmännisch vom Sofa aus.","Wenn Faulheit eine Grafikkarte wäre, hätte ich 500 FPS.","Wohltaten? Ich verteile höchstens dumme Sprüche.","Das ist kein Bug. Das ist ein Feature mit Alkoholproblem."};
 public MainWindow(){InitializeComponent();Left=settings.Left;Top=settings.Top;timer.Tick+=Tick;timer.Start();movementTimer.Tick+=Animate;movementTimer.Start();}
 void OnLoaded(object sender,RoutedEventArgs e){Say("Noxy ist wach. Wo sind meine verdammten Wohltaten?");}
 void Tick(object? sender,EventArgs e){seconds++;if(bubbleTime>0&&--bubbleTime==0)Bubble.Visibility=Visibility.Collapsed;
 var y=(int)(Math.Sin(seconds*.12)*3);Body.RenderTransform=new TranslateTransform(0,y);LeftEye.Height=RightEye.Height=settings.MovementMode=="sleep"||seconds%83==0?3:17;
 if(settings.MovementMode=="sleep"){Bubble.Visibility=Visibility.Collapsed;bubbleTime=0;return;}
 bool quiet=(settings.QuietWhenObs&&SystemMonitor.ObsRunning)||(settings.QuietWhenFullscreen&&SystemMonitor.ForegroundFullscreen());if(quiet){Bubble.Visibility=Visibility.Collapsed;bubbleTime=0;return;}
 bool f=SystemMonitor.FalloutRunning,w=SystemMonitor.WowRunning;
 if(f&&!lastFallout)Say("Fallout 76 gestartet. Das Ödland hat wieder Personalmangel.");
 else if(w&&!lastWow)Say("WoW gestartet. Viel Spaß beim virtuellen Überstundenmachen.");
 else if(settings.EnableSpeech&&seconds%Math.Max(15,settings.IntervalSeconds)==0){var phrase=phrases[random.Next(phrases.Length)];if(settings.EnableSensors&&SystemMonitor.RamPercent() is int ram&&ram>=85)phrase=$"RAM bei {ram} %. Du sammelst Programme wie Kronkorken!";Say(phrase);}
 lastFallout=f;lastWow=w;
 }
 void Animate(object? sender,EventArgs e){
  phase+=0.16;
  bool quiet=(settings.QuietWhenObs&&SystemMonitor.ObsRunning)||(settings.QuietWhenFullscreen&&SystemMonitor.ForegroundFullscreen());
  bool walking=settings.MovementMode is "walk" or "zippy" && !quiet&&!dragging;
  bool zippy=settings.MovementMode=="zippy";
  if(walking){
   if(idleTicks>0)idleTicks--;
   else {
    var area=SystemParameters.WorkArea;
    Left+=velocity*(zippy?2.6:1.0);
    if(Left<=area.Left){Left=area.Left;velocity=Math.Abs(velocity);}
    if(Left+Width>=area.Right){Left=Math.Max(area.Left,area.Right-Width);velocity=-Math.Abs(velocity);}
    if(++motionTicks>=random.Next(zippy?48:95,zippy?82:135)){motionTicks=0;idleTicks=random.Next(zippy?15:35,zippy?48:95);if(random.Next(3)==0)velocity=-velocity;}
   }
  }
  bool moving=walking&&idleTicks==0;
  double step=moving?Math.Sin(phase*2.5)*8:0;
  LeftFoot.RenderTransform=new TranslateTransform(0,step);
  RightFoot.RenderTransform=new TranslateTransform(0,-step);
  Body.RenderTransform=new TranslateTransform(0,moving?Math.Abs(Math.Sin(phase*2.5))*-3:Math.Sin(phase)*1.5);
 }
 void Say(string value){BubbleText.Text=value;Bubble.Visibility=Visibility.Visible;bubbleTime=11;}
 void DragWindow(object sender,MouseButtonEventArgs e){if(e.ChangedButton!=MouseButton.Left)return;if(e.ClickCount==2){Say(phrases[random.Next(phrases.Length)]);return;}try{dragging=true;DragMove();}catch(InvalidOperationException){}finally{dragging=false;}}
 void SayNow(object sender,RoutedEventArgs e)=>Say(phrases[random.Next(phrases.Length)]);
 void OpenChat(object sender,RoutedEventArgs e)=>new ChatWindow(settings).Show();
 void OpenSettings(object sender,RoutedEventArgs e)=>new SettingsWindow(settings,updates).ShowDialog();
 void ExitApp(object sender,RoutedEventArgs e)=>Close();
 void OnClosing(object? sender,System.ComponentModel.CancelEventArgs e){settings.Left=Left;settings.Top=Top;settings.Save();}
}

