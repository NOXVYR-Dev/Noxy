using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop;
namespace Noxy;
public partial class MainWindow : Window {
 readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromSeconds(1)};
 readonly DispatcherTimer movementTimer=new(){Interval=TimeSpan.FromMilliseconds(16)};
 readonly Random random=new();
 readonly UpdateService updates=new();
 readonly WeatherService weather=new();
 readonly PetSettings settings; readonly int slot;
 SettingsWindow? settingsWindow;
 string? activeProcess,pendingProcess; int pendingProcessSeconds=0; DateTime lastAppComment=DateTime.MinValue;
 readonly PetMotion motion=new();
 readonly DragVelocity dragVelocity=new();
 readonly PetShakeDetector shakeDetector=new();
 readonly Stopwatch animationClock=Stopwatch.StartNew();
 readonly TranslateTransform bodyOffset=new(), leftStep=new(), rightStep=new(), gazeLeft=new(), gazeRight=new();
 readonly RotateTransform bodyTilt=new();
 readonly RotateTransform leftEarWobble=new(),rightEarWobble=new();
 readonly ScaleTransform leftEarStretch=new(1,1),rightEarStretch=new(1,1);
 readonly ScaleTransform bodySquash=new(1,1);
 double lastFrame, phase, angerRemaining; bool dragging=false, quiet,shakeTriggered,angryFace;
 string appliedColor=""; int lastEncounterSlot=0, encounterReplySlot=0,lastShakePhrase=-1; DateTime lastEncounter=DateTime.MinValue, encounterReplyAt=DateTime.MinValue;
 int seconds=0, bubbleTime=0,nextSensorCommentSeconds=300; bool lastFallout=false,lastWow=false,updateNoticeShown=false;
 int? cpuPercent,gpuPercent;
 readonly string[] phrases={
  "Dein Desktop sieht aus, als hätte ein Supermutant aufgeräumt.","Ich habe heute nichts Produktives gemacht. Wir haben so viel gemeinsam.","Du klickst ziemlich selbstbewusst für jemanden ohne Plan.","Fallout im Herzen, ButzGatz im Blut. Und Chaos im Arbeitsspeicher.","Dein PC arbeitet. Ich überwache das fachmännisch vom Sofa aus.","Wenn Faulheit eine Grafikkarte wäre, hätte ich 500 FPS.","Wohltaten? Ich verteile höchstens dumme Sprüche.","Das ist kein Bug. Das ist ein Feature mit Alkoholproblem.",
  "Ich wollte dir helfen, aber dann fiel mir ein: Ich bin ein Desktop-Pet.","Dein Mauszeiger wirkt heute auch leicht überfordert.","Ich habe den Arbeitsspeicher gezählt. Der wollte nicht mit mir reden.","Falls du einen Plan hast: Ich stelle mich schon mal dagegen.","Du nennst es Multitasking. Ich nenne es fünf offene Baustellen.","Ich bin nicht faul. Ich spare professionell Energie.","Der Ladebalken und ich haben viel gemeinsam: Wir bewegen uns nur, wenn jemand zusieht.","Ich würde jetzt nicken, aber meine Animation ist gerade im Energiesparmodus.",
  "Du hast schon wieder ein Fenster geöffnet. Soll ich eins davon bewachen?", "Ich habe kurz nachgedacht. War anstrengend. Mach ich morgen wieder.","Wenn das Absicht war, war es mutig. Wenn nicht, auch.","Dein PC klingt beschäftigt. Ich bin es jedenfalls nicht.","Klick ruhig weiter. Irgendwann muss ja etwas Sinnvolles passieren.","Ich habe einen Witz über Updates. Der kommt gleich nach dem Neustart.","Mein Fitnessprogramm besteht aus einem Spaziergang über deinen Bildschirm.","Ich bin klein, flauschig und habe trotzdem mehr Überblick als dein Desktop.",
  "Das Fenster da hinten sieht wichtig aus. Ich ignoriere es für dich.","Du arbeitest wirklich hart daran, beschäftigt auszusehen.","Ich wollte einen Kaffee holen. Dann fiel mir ein, dass ich keine Hände habe.","Heute schon etwas erledigt? Ich auch nicht. Teamwork!", "Ich bewerte deine Fensterordnung mit: mutig.","Keine Sorge, ich passe auf. Worauf, weiß ich noch nicht.","Du brauchst keine Pause. Aber ich könnte eine gebrauchen.","Wenn du mich suchst: Ich bin genau da, wo du mich gestern vergessen hast.",
  "Ich habe heute schon drei Pixel bewegt. Bitte keine Überanstrengung.","Dein Desktop hat mehr offene Fenster als ich offene Fragen.","Ich bin nicht im Weg. Ich bin eine dekorative Herausforderung.","Gute Nachrichten: Ich habe nichts kaputt gemacht. Bis jetzt.","Das war bestimmt der Plan. Ich unterstütze ihn vorsichtshalber.","Du siehst konzentriert aus. Ich mache lieber keinen Lärm. Außer diesem Spruch.","Ich bin bereit für Abenteuer. Also für einen Spaziergang bis zum Bildschirmrand.","Bei dem Tempo wird sogar mein Bildschirmschoner nervös."
 };
 readonly string[] shakePhrases={
  "Hey, vorsichtig mit meinem Fell!","Uff! Ich bin ein Desktop-Pet, kein Cocktailshaker.","Mir dreht sich schon der ganze Desktop.",
  "Was soll das? Ich bin doch keine Schneekugel!","Einmal schütteln reicht. Ich bin wach!","Bitte nicht schütteln – mein Gehirn lädt schon langsam genug.",
  "Hilfe, der Bildschirm macht Purzelbäume!","Ich wackle freiwillig. Das hier war nicht abgesprochen.","Vorsicht! Gleich schüttle ich zurück.",
  "Wenn du mich schüttelst, wird mir pixelig.","Ich bin flauschig, nicht schüttelfest!","Okay, okay, ich hab's verstanden. Die Welt ist rund.",
  "Kann ich bitte wieder festen Desktop-Boden unter den Füßen haben?","Das gibt eine Beschwerde beim Mauszeiger.","Na warte, das merk ich mir. Vielleicht.",
  "Mein innerer Kompass zeigt jetzt in alle Richtungen.","Stopp! Meine Ohren sind schon ganz durcheinander.","Ich wollte sowieso kurz durchgeschüttelt werden. Also: nein."
 };
 public MainWindow(PetSettings petSettings,int instanceSlot){settings=petSettings;slot=instanceSlot;nextSensorCommentSeconds=random.Next(240,361);InitializeComponent();var pos=PetSettings.LoadPosition(slot);Left=pos?.Left??(settings.Left+((slot-1)*310));Top=pos?.Top??settings.Top;if(!double.IsFinite(Left))Left=100;if(!double.IsFinite(Top))Top=100;Title=$"Noxy #{slot}";var transforms=new TransformGroup();transforms.Children.Add(bodySquash);transforms.Children.Add(bodyTilt);transforms.Children.Add(bodyOffset);PetBody.RenderTransform=transforms;LeftFoot.RenderTransform=leftStep;RightFoot.RenderTransform=rightStep;LeftEye.RenderTransform=gazeLeft;RightEye.RenderTransform=gazeRight;var leftEarTransforms=new TransformGroup();leftEarTransforms.Children.Add(leftEarStretch);leftEarTransforms.Children.Add(leftEarWobble);var rightEarTransforms=new TransformGroup();rightEarTransforms.Children.Add(rightEarStretch);rightEarTransforms.Children.Add(rightEarWobble);LeftEar.RenderTransform=leftEarTransforms;RightEar.RenderTransform=rightEarTransforms;LeftEar.RenderTransformOrigin=RightEar.RenderTransformOrigin=new System.Windows.Point(.5,.85);LocationChanged+=(_,_)=>{if(dragging){var now=animationClock.Elapsed.TotalSeconds;var position=new System.Windows.Point(Left,Top);dragVelocity.Add(position,now);if(!shakeTriggered&&shakeDetector.Add(position,now))BecomeAngry();}};Closed+=(_,_)=>{timer.Stop();movementTimer.Stop();updates.Changed-=OnUpdateChanged;};ApplyNoxyColor();updates.Changed+=OnUpdateChanged;timer.Tick+=Tick;timer.Start();movementTimer.Tick+=Animate;movementTimer.Start();}
 void OnUpdateChanged(){if(updates.UpdateAvailable&&!updateNoticeShown){updateNoticeShown=true;Say($"Noxy {updates.Status} Rechtsklick auf mich und öffne Einstellungen zum Aktualisieren.");}else if(!updates.UpdateAvailable){updateNoticeShown=false;}}
 void OnLoaded(object sender,RoutedEventArgs e){var area=CurrentMonitorWorkArea();Left=Math.Clamp(Left,area.Left,area.Right);Top=Math.Clamp(Top,area.Top,area.Bottom);RefreshQuiet();lastFrame=animationClock.Elapsed.TotalSeconds;Say("Noxy ist wach. Wo sind meine verdammten Wohltaten?");}
 void Tick(object? sender,EventArgs e){seconds++;if(seconds%5==0){var latest=PetSettings.Load();if(latest.NoxyColors is {Count:>=3})settings.NoxyColors=latest.NoxyColors;settings.MaxInstances=latest.MaxInstances;if(slot>latest.MaxInstances){Close();return;}}ApplyNoxyColor();if(seconds%2==0)CheckForEncounter();ReplyToEncounter();if(bubbleTime>0&&--bubbleTime==0)Bubble.Visibility=Visibility.Collapsed;
 RefreshQuiet();
 if(settings.MovementMode=="sleep"){Bubble.Visibility=Visibility.Collapsed;bubbleTime=0;return;}
 if(quiet){Bubble.Visibility=Visibility.Collapsed;bubbleTime=0;return;}
 if(seconds>=120&&settings.EnableWeatherComments&&settings.LastWeatherAttemptDate?.Date!=DateTime.Today) _=AnnounceDailyWeatherAsync();
 ObserveForegroundProgram();
 bool f=SystemMonitor.FalloutRunning,w=SystemMonitor.WowRunning;
 if(f&&!lastFallout)Say("Fallout 76 gestartet. Das Ödland hat wieder Personalmangel.");
 else if(w&&!lastWow)Say("WoW gestartet. Viel Spaß beim virtuellen Überstundenmachen.");
 else if(settings.EnableSpeech&&seconds%Math.Max(15,settings.IntervalSeconds)==0){var phrase=phrases[random.Next(phrases.Length)];if(settings.EnableSensors&&SystemMonitor.RamPercent() is int ram&&ram>=85)phrase=$"RAM bei {ram} %. Du sammelst Programme wie Kronkorken!";Say(phrase);}
 if(settings.EnableSensors){cpuPercent=SystemMonitor.CpuPercent();gpuPercent=SystemMonitor.GpuPercent();}
 if(settings.EnableSensors&&seconds>=nextSensorCommentSeconds){Say(SystemUsageComment());nextSensorCommentSeconds=seconds+random.Next(240,361);}
 lastFallout=f;lastWow=w;
 }
 string SystemUsageComment(){
  var cpu=cpuPercent is int c?$"CPU {c} %":"CPU gerade nicht messbar";
  var gpu=gpuPercent is int g?$"GPU-Spitze {g} %":"GPU gerade nicht messbar";
  var ram=SystemMonitor.RamPercent() is int r?$"RAM {r} %":"RAM gerade nicht messbar";
  return $"PC-Check: {cpu}, {gpu}, {ram}.";
 }
 async Task AnnounceDailyWeatherAsync(){
  if(!settings.EnableWeatherComments||settings.LastWeatherAttemptDate?.Date==DateTime.Today)return;
  settings.LastWeatherAttemptDate=DateTime.Today;
  try{settings.Save();}catch{return;}
  var comment=await weather.GetDailyCommentAsync();
  if(settings.EnableWeatherComments&&comment is not null)Say(comment);
 }
 void ApplyNoxyColor(){if(settings.NoxyColors is not {Count:>=3})return;var colorText=settings.NoxyColors[Math.Clamp(slot-1,0,2)];if(string.Equals(appliedColor,colorText,StringComparison.OrdinalIgnoreCase))return;try{var color=(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorText);var brush=new SolidColorBrush(color);Body.Fill=new SolidColorBrush(System.Windows.Media.Color.FromRgb(36,33,45));Body.Stroke=brush;LeftEar.Fill=new SolidColorBrush(System.Windows.Media.Color.FromRgb(28,23,36));RightEar.Fill=new SolidColorBrush(System.Windows.Media.Color.FromRgb(28,23,36));LeftEar.Stroke=brush;RightEar.Stroke=brush;LeftEye.Fill=brush;RightEye.Fill=brush;LeftFoot.Fill=new SolidColorBrush(System.Windows.Media.Color.FromRgb(21,19,26));RightFoot.Fill=new SolidColorBrush(System.Windows.Media.Color.FromRgb(21,19,26));LeftFoot.Stroke=brush;RightFoot.Stroke=brush;Bubble.BorderBrush=brush;BubbleText.Foreground=brush;appliedColor=colorText;}catch{}}
 void CheckForEncounter(){var other=PetEncounterService.FindOverlappingSlot(slot,new WindowInteropHelper(this).Handle);if(other==0){lastEncounterSlot=0;encounterReplySlot=0;return;}if(other==lastEncounterSlot||DateTime.UtcNow-lastEncounter<TimeSpan.FromSeconds(30))return;lastEncounterSlot=other;lastEncounter=DateTime.UtcNow;if(slot<other){encounterReplySlot=0;Say(random.Next(3) switch{0=>"Na, du Penner. Auch hier?",1=>"Na, du Pappnase. Verfolgst du mich?",_=>"Guck an, der andere Lümmel ist auch da."});}else{encounterReplySlot=other;encounterReplyAt=DateTime.UtcNow.AddSeconds(3);}}
 void ReplyToEncounter(){if(encounterReplySlot==0||DateTime.UtcNow<encounterReplyAt)return;var other=encounterReplySlot;encounterReplySlot=0;if(PetEncounterService.FindOverlappingSlot(slot,new WindowInteropHelper(this).Handle)!=other)return;Say(random.Next(3) switch{0=>"Klar, du Pissfresse. Ich wohn jetzt auch hier.",1=>"Klar doch, du Knalltüte. Der Desktop ist groß genug.",_=>"Ja, du Schnarchnase. Wollte nur sehen, ob du noch da bist."});}
 void ObserveForegroundProgram(){
  var process=SystemMonitor.ForegroundProcessName();
  if(string.IsNullOrWhiteSpace(process)||process.Equals("Noxy",StringComparison.OrdinalIgnoreCase)||process.Equals("ApplicationFrameHost",StringComparison.OrdinalIgnoreCase)){pendingProcess=null;pendingProcessSeconds=0;return;}
  if(process.Equals(activeProcess,StringComparison.OrdinalIgnoreCase)){pendingProcess=null;pendingProcessSeconds=0;return;}
  if(!process.Equals(pendingProcess,StringComparison.OrdinalIgnoreCase)){pendingProcess=process;pendingProcessSeconds=1;return;}
  if(++pendingProcessSeconds<3)return;
  activeProcess=process;pendingProcess=null;pendingProcessSeconds=0;
  if(!settings.EnableAppComments||DateTime.UtcNow-lastAppComment<TimeSpan.FromMinutes(3))return;
  lastAppComment=DateTime.UtcNow;
  Say(AppComment(process));
 }
 static string AppComment(string process)=>process.ToLowerInvariant() switch{
  "chrome" or "msedge" or "firefox" or "opera" or "brave" or "vivaldi"=>"Schon wieder im Browser. Ich hoffe, du weißt noch, wonach du gesucht hast.",
  "discord"=>"Discord ist offen. Ich höre natürlich nicht mit. Ich habe Besseres zu tun.",
  "steam"=>"Steam gestartet. Nur kurz schauen, was es Neues gibt, hm?",
  "spotify"=>"Musik läuft. Damit klingt sogar Arbeit fast freiwillig.",
  "obs64" or "obs32"=>"OBS ist offen. Ich benehme mich jetzt besonders professionell.",
  "fallout76"=>"Fallout 76 läuft. Das Ödland hat wieder Personalmangel.",
  "wow" or "wowclassic" or "wow-64"=>"World of Warcraft ist offen. Virtuelle Überstunden zählen auch.",
  "code"=>"Visual Studio Code läuft. Zwischen den Bugs sieht es produktiv aus.",
  "devenv"=>"Visual Studio geöffnet. Möge der Build beim ersten Versuch durchlaufen.",
  "photoshop"=>"Photoshop offen. Ein paar Pixel brauchen bestimmt dringend deine Aufsicht.",
  "premierepro"=>"Premiere Pro läuft. Ich warte geduldig auf den Export.",
  _=>$"Du bist jetzt in {FriendlyProcessName(process)}. Ich passe hier auf."
 };
 static string FriendlyProcessName(string process){var name=process.Replace('_',' ').Replace('-',' ');return name.Length==0?"einem Programm":char.ToUpperInvariant(name[0])+name[1..];}
 void RefreshQuiet()=>quiet=(settings.QuietWhenObs&&SystemMonitor.ObsRunning)||(settings.QuietWhenFullscreen&&SystemMonitor.ForegroundFullscreen());
 void Animate(object? sender,EventArgs e){
  if(!IsLoaded)return;
  double now=animationClock.Elapsed.TotalSeconds,dt=Math.Clamp(now-lastFrame,0,.1);lastFrame=now;
  AdvanceAnimation(dt);
 }
 // Kept separate from the clock so the actual WPF animation can be tested deterministically.
 internal void AdvanceAnimation(double dt){
  bool blocked=quiet||dragging||ContextMenu?.IsOpen==true||settingsWindow?.IsVisible==true;
  if(!dragging){var next=motion.Advance(new System.Windows.Point(Left,Top),CurrentMonitorWorkArea(),dt,settings.MovementMode,blocked);Left=next.X;Top=next.Y;}
  bool sleeping=settings.MovementMode=="sleep";
  angerRemaining=Math.Max(0,angerRemaining-dt);bool angry=angerRemaining>0;
  LeftAngryBrow.Visibility=RightAngryBrow.Visibility=angry?Visibility.Visible:Visibility.Collapsed;
  if(angryFace!=angry){Mouth.Data=Geometry.Parse(angry?"M 75,137 Q 92,119 111,137":"M 75,124 Q 92,141 111,122");angryFace=angry;}
  phase+=dt*(motion.Moving?12:motion.Flying?16:2.5);
  double stride=motion.Moving?Math.Sin(phase):0;
  leftStep.X=stride*7;rightStep.X=-stride*7;
  leftStep.Y=-Math.Max(0,stride)*9;rightStep.Y=-Math.Max(0,-stride)*9;
  bodyOffset.Y=motion.Moving?-Math.Abs(Math.Cos(phase))*4:Math.Sin(phase)*(sleeping?.6:1.8);
  double earBounce=motion.Moving?Math.Abs(Math.Sin(phase)):angry?.22:.06;
  leftEarWobble.Angle=Math.Sin(phase*1.15)*(motion.Moving?25:angry?7:1.5);
  rightEarWobble.Angle=Math.Sin(phase*1.15+1.8)*(motion.Moving?22:angry?6:1.5);
  leftEarStretch.ScaleX=1-earBounce*.12;leftEarStretch.ScaleY=1+earBounce*.2;
  rightEarStretch.ScaleX=1+earBounce*.09;rightEarStretch.ScaleY=1-earBounce*.14;
  bodyTilt.Angle=motion.Flying?Math.Clamp(motion.VelocityX/35,-24,24):angry?Math.Sin(phase*14)*9:dragging?Math.Sin(phase*2)*5:stride*2;
  bodySquash.ScaleX=1+motion.Impact*.12;
  bodySquash.ScaleY=1-motion.Impact*.1;
  double blink=animationClock.Elapsed.TotalSeconds%5.3;
  LeftEye.Height=RightEye.Height=sleeping?3:blink<.16?3:angry?13:17;
  gazeLeft.X=gazeRight.X=sleeping?0:motion.Direction*3;
  SleepLabel.Visibility=sleeping?Visibility.Visible:Visibility.Collapsed;
  if(sleeping||quiet){Bubble.Visibility=Visibility.Collapsed;bubbleTime=0;}
 }
 Rect CurrentMonitorWorkArea(){
  var screen=System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea;
  // Screen.WorkingArea uses screen pixels while Window.Left/Top and ActualWidth/Height
  // use WPF device-independent units. Let WPF convert both monitor corners so mixed
  // DPI setups and negative monitor coordinates do not introduce a second scale error.
  var topLeft=PointFromScreen(new System.Windows.Point(screen.Left,screen.Top));
  var bottomRight=PointFromScreen(new System.Windows.Point(screen.Right,screen.Bottom));
  var minLeft=Left+topLeft.X;
  var minTop=Top+topLeft.Y;
  var maxLeft=Math.Max(minLeft,Left+bottomRight.X-ActualWidth);
  var maxTop=Math.Max(minTop,Top+bottomRight.Y-ActualHeight);
  if(!double.IsFinite(minLeft)||!double.IsFinite(minTop)||!double.IsFinite(maxLeft)||!double.IsFinite(maxTop))
   return new Rect(Left,Top,0,0);
  return new Rect(minLeft,minTop,maxLeft-minLeft,maxTop-minTop);
 }
 void Say(string value){if(settings.MovementMode=="sleep"||quiet)return;BubbleText.Text=value;Bubble.Visibility=Visibility.Visible;bubbleTime=11;}
 string NextShakePhrase(){int index;do{index=random.Next(shakePhrases.Length);}while(index==lastShakePhrase);lastShakePhrase=index;return shakePhrases[index];}
 void BecomeAngry(){if(quiet||settings.MovementMode=="sleep")return;shakeTriggered=true;angerRemaining=3.5;Say(NextShakePhrase());}
 void DragWindow(object sender,MouseButtonEventArgs e){
  if(e.ChangedButton!=MouseButton.Left)return;
  if(e.ClickCount==2){motion.Stop();OpenSettings(sender,e);return;}
  motion.Stop();dragging=true;shakeTriggered=false;var now=animationClock.Elapsed.TotalSeconds;var position=new System.Windows.Point(Left,Top);dragVelocity.Begin(position,now);shakeDetector.Begin(position,now);
  bool released=false;
  try{DragMove();released=Mouse.LeftButton==MouseButtonState.Released;}
  catch(InvalidOperationException){}
  finally{EndDrag(released);}
 }
 internal void EndDrag(bool released){
  dragging=false;RefreshQuiet();
  var area=CurrentMonitorWorkArea();Left=Math.Clamp(Left,area.Left,area.Right);Top=Math.Clamp(Top,area.Top,area.Bottom);
  if(released&&!quiet&&settings.MovementMode!="sleep"&&!shakeTriggered)motion.Throw(dragVelocity.Release(animationClock.Elapsed.TotalSeconds));
  lastFrame=animationClock.Elapsed.TotalSeconds;
 }
 void SayNow(object sender,RoutedEventArgs e)=>Say(phrases[random.Next(phrases.Length)]);
 void ApplyInstanceLimit(int count){if(slot>count){Close();return;}foreach(var freeSlot in ProcessInstanceManager.GetFreeSlots(count))try{var path=Environment.ProcessPath;if(!string.IsNullOrWhiteSpace(path))Process.Start(new ProcessStartInfo(path){UseShellExecute=true});}catch{} }
 void OpenSettings(object sender,RoutedEventArgs e){try{if(settingsWindow is { IsVisible:true }){settingsWindow.Activate();return;}settingsWindow=new SettingsWindow(settings,updates,ApplyInstanceLimit){Owner=this};settingsWindow.Closed+=(_,_)=>settingsWindow=null;settingsWindow.Show();settingsWindow.Activate();}catch(Exception ex){settingsWindow=null;System.Windows.MessageBox.Show($"Die Einstellungen konnten nicht geöffnet werden.\n\n{ex.Message}","Noxy",MessageBoxButton.OK,MessageBoxImage.Error);}}
 void ExitApp(object sender,RoutedEventArgs e)=>Close();
 void OnClosing(object? sender,System.ComponentModel.CancelEventArgs e){settings.SavePosition(slot,Left,Top);}
}

