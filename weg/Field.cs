using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml.Linq;

namespace ations
{
  public class Field
  {
    public String Type { get; set; }
    public int Index { get; set; }
    public ObservableCollection<string> TypesAllowed { get; set; }
    public Thickness Margin { get; set; }
    public XElement X { get; set; }
  }
}
