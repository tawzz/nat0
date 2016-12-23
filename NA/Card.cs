using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ations
{
  public class Card : DependencyObject, INotifyPropertyChanged
  {
    public string Type { get; set; }
    public string Name { get; set; }
    public Brush Brush { get; set; }
    public int Age { get; set; }
    public BitmapImage Image { get; set; }
    public BitmapImage ImageDeployObject { get; set; }
    public XElement X { get; set; }
    public int Cost { get; set; }

    public int NumDeployed { get { return numDeployed; } set { if (numDeployed != value) { numDeployed = value; NotifyPropertyChanged(); } } }
    int numDeployed; // includes architects on wonder in construction
    public bool CanBuy { get { return canBuy; } set { if (canBuy != value) { canBuy = value; NotifyPropertyChanged(); } } }
    bool canBuy;
    public bool CanActivate { get { return canActivate; } set { if (canActivate != value) { canActivate = value; NotifyPropertyChanged(); } } }
    bool canActivate;
    public bool IsMarked { get { return isMarked; } set { if (isMarked != value) { isMarked = value; NotifyPropertyChanged(); } } }
    bool isMarked;
    public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(Card), null);

    public static Card MakeEmptyCard(Field field, string type)
    {
      var card = new Card();

      card.Type = type;
      card.Brush = CardType.typeColors[card.Type];
      card.Age = 1;
      card.Image = Helpers.GetEmptyCardImage(type);
      card.X = field.X;
      field.Card = card;

      return card;
    }
    public static Card MakeCivCard(Field field, string civ)
    {
      /* example of cards in a civ xml:
  <cards>
    <card type="advisor"></card>
    <card name="buffalo_horde" type="building"></card>
    <card name="teepee" type="building"></card>
    <card name="brave" type="military"></card>
    <card name="adobe" type="building"></card>
    <card name="america_dyn1" type="dynasty"></card>
    <card name="" type="wic"></card>
    <card name="" type="colony"></card>
    <card name="" type="colony"></card>
    <card name="" type="wonder"></card>
    <card name="" type="wonder"></card>
    <card name="" type="wonder"></card>
    <card name="" type="wonder"></card>
    <card name="" type="wonder"></card>
  </cards>
  */

      var card = new Card();
      var name = field.X.astring("name");

      if (string.IsNullOrEmpty(name)) // empty field make empty card
      {
        card.Type = field.Type;
        card.Brush = CardType.typeColors[card.Type];
        card.Age = 1;
        card.Image = Helpers.GetEmptyCardImage(field.Type);
        card.X = field.X;
      }
      else
      {
        //search for long xml info in either _commoncards.xml, [civ]cards.xml, or age 1 cards
        var xcard = Helpers.GetCivCardX(civ, name)
          ?? Helpers.GetCommonCardX(name)
          ?? Helpers.GetCardX(name, 1);
        if (xcard == null)
        {
          throw new System.Exception("card " + name + " not found (xml) in MakeCivCard");
        }
        else
        {
          card.Type = xcard.astring("type");
          card.Brush = CardType.typeColors[card.Type];
          card.Name = name;
          card.Age = xcard.aint("age", 1); // sollte eh immer 1 sein
          card.Image = card.dyn() ? Helpers.GetDynCardImage(civ) : Helpers.GetCivCardImage(name);
          card.ImageDeployObject = card.buildmil() ? Helpers.GetMiscImage("worker") :
            card.wonder() ? Helpers.GetMiscImage("architect") : Helpers.GetMiscImage("cross");
          card.X = xcard;
        }
      }

      field.Card = card;

      //card.CanActivate = true;//testing
      return card;

    }
    public static Card MakeCard(XElement xcard)
    {
      var name = xcard.astring("name");
      var type = xcard.astring("type");
      int age = xcard.aint("age", 1);
      var card = new Card
      {
        Type = type,
        Brush = CardType.typeColors[type],
        Name = name,
        Age = age,
        X = xcard,
        Image = Helpers.GetCardImage(name, age),
      };
      card.ImageDeployObject = card.buildmil() ? Helpers.GetMiscImage("worker") :
          card.wonder() ? Helpers.GetMiscImage("architect") : Helpers.GetMiscImage("cross");

      return card;
    }
    public static Card MakeProgressCard(XElement xcard, Field field)
    {
      var card = MakeCard(xcard);
      //var name = xcard.astring("name");
      //var type = xcard.astring("type");
      //int age = xcard.aint("age", 1);
      //var card = new Card
      //{
      //  Type = type,
      //  Brush = CardType.typeColors[type],
      //  Name = name,
      //  Age = age,
      //  X = xcard,
      //  Image = Helpers.GetCardImage(name, age),
      //};
      //card.ImageDeployObject = card.buildmil() ? Helpers.GetMiscImage("worker") :
      //    card.wonder() ? Helpers.GetMiscImage("architect") : Helpers.GetMiscImage("cross");
      field.Card = card;

      //card.CanBuy = true; //testing
      return card;
    }
    public static Card MakeEventCard(XElement xcard)
    {
      var card = new Card();
      card.Type = xcard.astring("type", "event");
      card.Brush = CardType.typeColors[card.Type];
      card.Name = xcard.astring("name");
      card.Age = xcard.aint("age", 1);
      card.X = xcard;
      card.Image = Helpers.GetEventCardImage(card.Name, card.Age);
      return card;
    }
    public static Card MakeEventCard(string name)
    {
      var xcard = Helpers.GetEventCardX(name);
      if (xcard != null)
      {
        var card = new Card();
        card.Type = xcard.astring("type", "event");
        card.Brush = CardType.typeColors[card.Type];
        card.Name = xcard.astring("name");
        card.Age = xcard.aint("age", 1);
        card.X = xcard;
        card.Image = Helpers.GetEventCardImage(card.Name, card.Age);
        return card;
      }
      return null;
    }
    public static Card MakeCard(string name = "archimedes", int age = 1) //for testing
    {
      var xcard = Helpers.GetCardX(name, age);
      var type = xcard.astring("type");
      var card = new Card
      {
        Type = type,
        Brush = CardType.typeColors[type],
        Name = name,
        Age = age,
        X = xcard,
        Image = Helpers.GetCardImage(name, age),
      };
      card.ImageDeployObject = card.buildmil() ? Helpers.GetMiscImage("worker") :
          card.wonder() ? Helpers.GetMiscImage("architect") : Helpers.GetMiscImage("cross");

      return card;
    }

    //access info on this card from xml
    public int[] GetArchCostArray { get { return X.astring("arch").Split('_').Select(x => int.Parse(x)).ToArray(); } }
    public int GetMilitary { get { return X.aint("military", 0); } }
    public int GetStability { get { return X.aint("stability", 0); } }




    //TODO: mach daraus non static
    //public static int[] GetArchCostArray(Card card) { return GetArchCostArray(card.X); }
    //public static int[] GetArchCostArray(XElement xcard) { return xcard.astring("arch").Split('_').Select(x => int.Parse(x)).ToArray(); }


    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Name + "(" + Type + ")"; }
    #endregion
  }
}
