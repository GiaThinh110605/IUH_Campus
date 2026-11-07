using System;
using System.Collections.Generic;
using UnityEngine;
namespace IUHCampus.Guardian.Progress {
 [CreateAssetMenu(menuName="IUH Guardian/Item Database")]
 public sealed class GuardianItemDatabase : ScriptableObject {
  public List<GuardianItemDefinition> definitions=new List<GuardianItemDefinition>();
  public void Validate(){var ids=new HashSet<string>(StringComparer.Ordinal);foreach(var d in definitions)if(d==null||string.IsNullOrWhiteSpace(d.id)||!ids.Add(d.id)||d.maxStack<1)throw new ArgumentException("Invalid or duplicate item id in Guardian database.");}
  public GuardianItemDefinition Find(string id){return definitions.Find(d=>d!=null&&d.id==id);}
  void OnValidate(){try{Validate();}catch(ArgumentException e){Debug.LogWarning("[IUH Guardian] "+e.Message,this);}}
 }
}
