using System.IO;
using System.Text.Json;
namespace Noxy;
public class PetSettings {
 public int IntervalSeconds {get;set;}=75;
 public int MaxInstances {get;set;}=1;
 public List<string> NoxyColors {get;set;}=["#FFFF425C","#FF35D8C5","#FFFFB52E"];
 public bool QuietWhenObs {get;set;}=true;
 public bool QuietWhenFullscreen {get;set;}=true;
 public bool EnableSpeech {get;set;}=true;
 public bool EnableAppComments {get;set;}=true;
 public bool EnableWeatherComments {get;set;}=false;
 public DateTime? LastWeatherAttemptDate {get;set;}
 public bool EnableWalking {get;set;}=true;
 public string MovementMode {get;set;}="walk";
 public bool EnableSensors {get;set;}=true;
 public string Model {get;set;}="gpt-4.1-mini";
 public double Left {get;set;}=100;
 public double Top {get;set;}=100;
 public static string DirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Noxy");
 public static string FilePath => Path.Combine(DirectoryPath,"settings.json");
 private static string LegacyFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Noxvyr","settings.json");
 public static PetSettings Load(){try {if(File.Exists(FilePath)) return LoadFile(FilePath);if(File.Exists(LegacyFilePath)){var old=LoadFile(LegacyFilePath);old.Save();return old;}}catch{}return new();}
 public static (double Left,double Top)? LoadPosition(int slot){try{var path=Path.Combine(DirectoryPath,$"position-{slot}.json");if(!File.Exists(path))return null;using var d=JsonDocument.Parse(File.ReadAllText(path));return(d.RootElement.GetProperty("left").GetDouble(),d.RootElement.GetProperty("top").GetDouble());}catch{return null;}}
 public void SavePosition(int slot,double left,double top){Directory.CreateDirectory(DirectoryPath);File.WriteAllText(Path.Combine(DirectoryPath,$"position-{slot}.json"),JsonSerializer.Serialize(new{left,top}));}
 private static PetSettings LoadFile(string path){var json=File.ReadAllText(path);var settings=JsonSerializer.Deserialize<PetSettings>(json)??new();using var document=JsonDocument.Parse(json);if(!document.RootElement.TryGetProperty(nameof(MovementMode),out _))settings.MovementMode=settings.EnableWalking?"walk":"static";if(settings.MovementMode is not ("walk" or "zippy" or "static" or "sleep"))settings.MovementMode="walk";return settings;}
 public void Save(){Directory.CreateDirectory(DirectoryPath);using var mutex=new Mutex(false,"Local\\Noxy.Settings");mutex.WaitOne();try{File.WriteAllText(FilePath,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));}finally{mutex.ReleaseMutex();}}
}

