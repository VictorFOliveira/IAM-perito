using System.Diagnostics;
using System.Text;
namespace IAM.Perito.Desktop;
public sealed class AdbService {
 public async Task<(int Code,string Output)> RunAsync(string[] args,CancellationToken token) {
  var start=new ProcessStartInfo("adb"){RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false,CreateNoWindow=true};
  foreach(var arg in args) start.ArgumentList.Add(arg);
  using var process=new Process{StartInfo=start};
  process.Start();
  try {
   var stdout=process.StandardOutput.ReadToEndAsync(token);
   var stderr=process.StandardError.ReadToEndAsync(token);
   await process.WaitForExitAsync(token);
   var output=(await stdout)+Environment.NewLine+(await stderr);
   return (process.ExitCode,output.Trim());
  } catch { if(!process.HasExited) process.Kill(entireProcessTree:true); throw; }
 }
 public async Task<string> AuthorizedSerialAsync(CancellationToken token){
  var (code,output)=await RunAsync(new[]{ "devices" },token);
  if(code!=0) throw new InvalidOperationException(output);
  var devices=output.Split('\\n').Select(s=>s.Trim()).Where(s=>s.EndsWith("\\tdevice",StringComparison.Ordinal)).ToList();
  if(devices.Count!=1) throw new InvalidOperationException("É necessário exatamente um Android autorizado no ADB. Verifique 'adb devices' (estado device).");
  return devices[0].Split('\\t')[0];
 }
 public async Task<IReadOnlyList<PhotoItem>> FindPhotosAsync(string serial,CancellationToken token){
  // find limita a pesquisa ao armazenamento compartilhado; não examina blocos apagados.
  const string script="find /sdcard/DCIM /sdcard/Pictures -type f \\( -iname '*.jpg' -o -iname '*.jpeg' -o -iname '*.png' -o -iname '*.webp' -o -iname '*.heic' -o -iname '*.heif' \\) 2>/dev/null";
  var (code,output)=await RunAsync(new[]{ "-s",serial,"shell","sh","-c",script },token);
  if(code!=0 && string.IsNullOrWhiteSpace(output)) throw new InvalidOperationException("Não foi possível listar as mídias.");
  return output.Split('\\n',StringSplitOptions.RemoveEmptyEntries).Select(p=>p.Trim()).Where(p=>p.StartsWith("/sdcard/",StringComparison.Ordinal) && !p.Contains('\\r') && !p.Contains('\\0')).Distinct(StringComparer.Ordinal).Select(p=>new PhotoItem(Path.GetFileName(p),p,p.Contains("thumb",StringComparison.OrdinalIgnoreCase)?"Possível miniatura":"Arquivo existente")).ToList();
 }
}
public sealed record PhotoItem(string Name,string RemotePath,string Classification);