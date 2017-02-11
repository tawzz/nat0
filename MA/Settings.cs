using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ations.MA
{
  public sealed class Settings
  {
    static XElement map = null;
    static bool modified;

    public static void Initialize() { map = Helpers.LoadFromFile("settings.xml"); if (map == null) map = new XElement("settings"); modified = false; }

    public static void Save() { modified = true; if (modified) { Helpers.SaveToFile(map, "settings.xml"); modified = false; } }

    public static string Get(string key) { return map.Element(key) != null ? map.Element(key).Value : null; }

    public static bool GetBool(string key, bool defaultValue = false) { return map.Element(key) != null ? map.Element(key).Value.ToLower() == "true" : defaultValue; }

    public static int GetInt(string key, int defaultValue = 0) { int result; return (map.Element(key) != null) && int.TryParse(map.Element(key).Value, out result) ? result : defaultValue; }

    public static string GetString(string key, string defaultValue = "") { return map.Element(key) != null ? map.Element(key).Value : defaultValue; }

    public static XElement GetElement(string key) { return map.Element(key); }

    public static string Consume(string key) { var res = Get(key); Erase(key); return res; }

    //setters return true if value has changed
    public static bool SetInt(string key, int val) { return Set(key, val.ToString()); }

    public static bool SetInt(string key, string value, int min, int max) { int val; return (int.TryParse(value, out val) && val >= min && val <= max) ? Set(key, value) : false; }

    public static bool Set(string key, string value)
    {
      string val = map.Element(key) != null ? val = map.Element(key).Value : null;

      if (val == value) return false; // no need to set

      if (val != null)
        map.Element(key).Value = value;
      else
        map.Add(new XElement(key, value));
      modified = true;
      return true;
    }

    public static bool Set(string key, XElement xelement) { if (map.Element(key) != null) map.Element(key).Remove(); map.Add(xelement); modified = true; return true; }

    public static void Erase(string key) { if (map.Element(key) != null) { map.Element(key).Remove(); modified = true; } }

    public static void Clear() { map = new XElement("settings"); modified = true; }
  }
}
