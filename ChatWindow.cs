using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace Noxy;
public class ChatWindow:Window {
 readonly System.Windows.Controls.TextBox log=new(){IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
 readonly System.Windows.Controls.TextBox input=new(){Height=55,TextWrapping=TextWrapping.Wrap,AcceptsReturn=true};
 readonly PasswordBox key=new(); readonly System.Windows.Controls.Button send=new(){Content="Senden",Height=34};
 readonly PetSettings settings;
 public ChatWindow(PetSettings s){settings=s;Title="Noxy | KI-Chat (optional)";Width=480;Height=540;WindowStartupLocation=WindowStartupLocation.CenterScreen;
 var grid=new Grid{Margin=new Thickness(12)};Content=grid;foreach(var h in new[]{GridLength.Auto,new GridLength(1,GridUnitType.Star),GridLength.Auto,GridLength.Auto})grid.RowDefinitions.Add(new RowDefinition{Height=h});
 var head=new StackPanel();head.Children.Add(new TextBlock{Text="OpenAI API-Key (nur für diese Sitzung, nicht gespeichert):"});key.Margin=new Thickness(0,5,0,9);head.Children.Add(key);Grid.SetRow(head,0);grid.Children.Add(head);
 Grid.SetRow(log,1);grid.Children.Add(log);log.Text="Noxy: Na, was willst du? Ich bin künstlich intelligent, nicht künstlich geduldig.\n\nFür den KI-Chat brauchst du einen eigenen OpenAI API-Key. Dabei können Kosten entstehen.\n";
 Grid.SetRow(input,2);input.Margin=new Thickness(0,8,0,8);grid.Children.Add(input);Grid.SetRow(send,3);grid.Children.Add(send);send.Click+=async(_,_)=>await Send();
 }
 async Task Send(){var question=input.Text.Trim();var apiKey=key.Password.Trim();if(question.Length==0)return;if(apiKey.Length==0){System.Windows.MessageBox.Show("Bitte zuerst einen OpenAI API-Key eingeben.");return;}input.Clear();log.AppendText("\nDu: "+question+"\n");send.IsEnabled=false;
 try{using var client=new HttpClient{Timeout=TimeSpan.FromSeconds(35)};client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",apiKey);
 var body=new {model=settings.Model,messages=new object[]{new {role="system",content="Du bist Noxy, ein durchgeknalltes, freches, sarkastisches deutsches Desktop-Pet. Antworte knapp, witzig, mit bissigem Humor, aber nicht hasserfüllt. Keine erfundenen PC-Messwerte. Du bist ein kleines schwarz-rotes Monster."},new {role="user",content=question}}};
 using var response=await client.PostAsync("https://api.openai.com/v1/chat/completions",new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json"));var raw=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode)throw new Exception($"API-Fehler {(int)response.StatusCode}: "+raw[..Math.Min(raw.Length,280)]);
 using var json=JsonDocument.Parse(raw);var answer=json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();log.AppendText("Noxy: "+answer+"\n");log.ScrollToEnd();
 }catch(Exception ex){log.AppendText("Fehler: "+ex.Message+"\n");}finally{send.IsEnabled=true;}
 }
}

