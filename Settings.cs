using System.IO;
using System.Text.Json;
namespace Noxy;
public class PetSettings {
 public int IntervalSeconds {get;set;}=75;
 public bool QuietWhenObs {get;set;}=true;
 public bool QuietWhenFullscreen {get;set;}=true;
 public bool EnableSpeech {get;set;}=true;
 public bool EnableWalking {get;set;}=true;
 public bool EnableSensors {get;set;}=true;
 public string Model {get;set;}="gpt-4.1-mini";
 public double Left {get;set;}=100;
 public double Top {get;set;}=100;
 public static string DirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Noxy");
 public static string FilePath => Path.Combine(DirectoryPath,"settings.json");
 private static string LegacyFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Noxvyr","settings.json");
 public static PetSettings Load(){try {if(File.Exists(FilePath)) return JsonSerializer.Deserialize<PetSettings>(File.ReadAllText(FilePath))??new();if(File.Exists(LegacyFilePath)){var old=JsonSerializer.Deserialize<PetSettings>(File.ReadAllText(LegacyFilePath))??new();old.Save();return old;}}catch{}return new();}
 public void Save(){Directory.CreateDirectory(DirectoryPath);File.WriteAllText(FilePath,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));}
}

