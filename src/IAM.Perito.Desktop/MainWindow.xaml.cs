using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
namespace IAM.Perito.Desktop;
public partial class MainWindow : Window {
 private readonly AdbService adb=new();
 private readonly ObservableCollection<PhotoItem> photos=new();
 private CancellationTokenSource? active;
 private string? serial;
 public MainWindow(){InitializeComponent();PhotosGrid.ItemsSource=photos;}
 private void Log(string msg){LogBox.AppendText($"[{DateTimeOffset.Now:O}] {msg}{Environment.NewLine}");LogBox.ScrollToEnd();}
 private async Task ExecuteAsync(Func<CancellationToken,Task> work){
  if(active!=null){MessageBox.Show("Já existe uma operação em andamento.");return;}
  active=new CancellationTokenSource();
  try{await work(active.Token);}catch(OperationCanceledException){StatusLabel.Text="Operação cancelada";Log("Cancelada");}
  catch(Exception ex){StatusLabel.Text=ex.Message;Log("ERRO: "+ex.Message);}
  finally{active.Dispose();active=null;}
 }
 private async void Detect_Click(object sender,RoutedEventArgs e)=>await ExecuteAsync(async ct=>{
  serial=await adb.AuthorizedSerialAsync(ct);
  var (_,model)=await adb.RunAsync(new[]{"-s",serial,"shell","getprop","ro.product.model"},ct);
  var (_,version)=await adb.RunAsync(new[]{"-s",serial,"shell","getprop","ro.build.version.release"},ct);
  DeviceLabel.Text=$"Dispositivo: {model.Trim()} • Android {version.Trim()}";
  StatusLabel.Text="Conectado e autorizado";Log("Dispositivo autorizado detectado.");
 });
 private async void Scan_Click(object sender,RoutedEventArgs e)=>await ExecuteAsync(async ct=>{
  serial=await adb.AuthorizedSerialAsync(ct);
  StatusLabel.Text="Pesquisando arquivos existentes...";
  var results=await adb.FindPhotosAsync(serial,ct);
  photos.Clear();foreach(var item in results)photos.Add(item);
  StatusLabel.Text=$"{photos.Count} arquivos encontrados. Isto não representa recuperação de arquivos apagados.";
  Log($"Busca concluída: {photos.Count} arquivos acessíveis.");
 });
 private async void Export_Click(object sender,RoutedEventArgs e)=>await ExecuteAsync(async ct=>{
  if(string.IsNullOrWhiteSpace(serial))throw new InvalidOperationException("Detecte o aparelho primeiro.");
  var chosen=PhotosGrid.SelectedItems.Cast<PhotoItem>().ToArray();
  if(chosen.Length==0)throw new InvalidOperationException("Selecione ao menos uma fotografia.");
  var dialog=new OpenFolderDialog{Title="Selecione a pasta para salvar o caso"};
  if(dialog.ShowDialog()!=true)return;
  var caseDir=Path.Combine(dialog.FolderName,"IAM-PERITO-"+DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")[..8]);
  Directory.CreateDirectory(caseDir);
  var records=new List<object>();
  for(int i=0;i<chosen.Length;i++){
   ct.ThrowIfCancellationRequested();
   var item=chosen[i];
   var dest=Path.Combine(caseDir,$"{i:D5}_"+SafeName(item.Name));
   var (code,output)=await adb.RunAsync(new[]{"-s",serial,"pull",item.RemotePath,dest},ct);
   if(code!=0||!File.Exists(dest)){Log("Falha na coleta: "+item.RemotePath+" "+output);continue;}
   await using var stream=File.OpenRead(dest);
   var hash=Convert.ToHexString(await SHA256.HashDataAsync(stream,ct));
   records.Add(new{source=item.RemotePath,local=Path.GetFileName(dest),classification=item.Classification,size=new FileInfo(dest).Length,sha256=hash,acquiredUtc=DateTimeOffset.UtcNow});
   StatusLabel.Text=$"Exportando {i+1}/{chosen.Length}";
  }
  var manifest=new{tool="IAM-PERITO",version="0.1.0",method="ADB logical pull",deviceSerial=serial,createdUtc=DateTimeOffset.UtcNow,warning="Arquivos existentes/cópias acessíveis; não comprova recuperação de exclusões permanentes.",files=records};
  await File.WriteAllTextAsync(Path.Combine(caseDir,"manifest.json"),JsonSerializer.Serialize(manifest,new JsonSerializerOptions{WriteIndented=true}),ct);
  StatusLabel.Text=$"Coleta finalizada: {records.Count} arquivos. Pasta: {caseDir}";
  Log(StatusLabel.Text);
 });
 private void Cancel_Click(object sender,RoutedEventArgs e)=>active?.Cancel();
 private static string SafeName(string name){
  var invalid=Path.GetInvalidFileNameChars();
  var safe=new string(name.Select(c=>invalid.Contains(c)?'_':c).ToArray());
  return string.IsNullOrWhiteSpace(safe)?"arquivo.bin":safe;
 }
}