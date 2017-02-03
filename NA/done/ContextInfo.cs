using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ations
{
  public class ContextInfo
  {
    public ctx Id { get; set; }
    public List<Step> Steps { get; set; }
    public string BaseMessage { get; set; }
    public Dictionary<cl[], Step[]> StepDictionary { get; set; }
  }
}
