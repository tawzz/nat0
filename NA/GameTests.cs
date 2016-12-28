using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public partial class Game
  {

    public async void Tester() { Title = "Testing..."; LongMessage = Message = "TESTING CHOICE PICKER!!!"; await TestChoices(); }
    public async Task TestChoices()
    {
      IsOkStartEnabled = false; IsRunning = true;

      var choice = await WaitForPickChoiceCompleted(new string[] { "pick 3 coal", "go last", "steal a card from opponent" }, "event");

      Message = choice.Text + " = your pick";
      //Choices.Add(new Choice { Text = "pick 3 gold" });
      //Choices.Add(new Choice { Text = "go first" });
      //ShowChoices = true;
      //await Task.Delay(longDelay);
      //await WaitForButtonClick();
      //var choice = SelectedChoice;
      //if (choice != null) Message = "you picked " + SelectedChoice.Text; else Message = "NO Choice picked!";

    }

  }
}
