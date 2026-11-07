using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace IUHCampus.Guardian.Progress {
 public sealed class GuardianProgressStore {
  public string Path {get;} bool warned,recovered;
  public GuardianProgressStore(string directory){Path=System.IO.Path.Combine(directory,"guardian_progress.json");}
  GuardianProgressState Read(string path){var json=File.ReadAllText(path);if(!json.Contains("\"version\""))throw new ArgumentException("Missing version.");var state=JsonUtility.FromJson<GuardianProgressState>(json);if(state==null)throw new ArgumentException("Missing state.");state.Validate();return state;}
  public GuardianProgressState Load(Func<GuardianProgressState> create){
   if(!File.Exists(Path)&&!File.Exists(Path+".bak"))return create();
   try{return Read(Path);}catch(Exception e)when(e is IOException||e is ArgumentException||e is UnauthorizedAccessException){}
   try{var state=Read(Path+".bak");recovered=true;Warn("[IUH Guardian] Progress recovered from backup.");return state;}catch(Exception e)when(e is IOException||e is ArgumentException||e is UnauthorizedAccessException){}
   Warn("[IUH Guardian] Progress files invalid; starting new progress.");return create();
  }
  void Warn(string message){if(warned)return;warned=true;Debug.LogWarning(message);}
  public void Save(GuardianProgressState state){state.Validate();Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));var bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(state,true));using(var file=new FileStream(Path+".tmp",FileMode.Create,FileAccess.Write,FileShare.None)){file.Write(bytes,0,bytes.Length);file.Flush(true);}
   if(File.Exists(Path))File.Replace(Path+".tmp",Path,recovered?null:Path+".bak");else File.Move(Path+".tmp",Path);recovered=false;
  }
 }
}
