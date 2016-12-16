using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ations
{
  public class ACard : INotifyPropertyChanged
  {
    public string Type { get; set; }
    public string Name { get; set; }
    public Brush Brush { get; set; }
    public int Age { get; set; }
    public BitmapImage Image { get; set; }
    public XElement X { get; set; }
    public int Cost { get; set; }

    public int NumDeployed { get { return numDeployed; } set { if (numDeployed != value) { numDeployed = value; NotifyPropertyChanged(); } } }
    int numDeployed; // includes architects on wonder in construction
    public bool CanBuy { get { return canBuy; } set { if (canBuy != value) { canBuy = value; NotifyPropertyChanged(); } } }
    bool canBuy;
    public bool CanPlace { get { return canPlace; } set { if (canPlace != value) { canPlace = value; NotifyPropertyChanged(); } } }
    bool canPlace;

    public static ACard MakeCivCard(AField field, string civ)
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

      var card = new ACard();
      var name = field.X.astring("name");

      //search for long xml info in either _commoncards.xml, [civ]cards.xml, or age 1 cards
      var xcard = Helpers.GetCivCardX(civ, name)
        ?? Helpers.GetCommonCardX(name)
        ?? Helpers.GetCardX(name, 1);

      if (xcard == null)
      {
        card.Type = field.Type;
        card.Brush = CT.typeColors[card.Type];
        card.Age = 1;
        card.Image = Helpers.GetEmptyCardImage(field.Type);
        card.X = field.X;
      }
      else
      {
        card.Type = xcard.astring("type");
        card.Brush = CT.typeColors[card.Type];
        card.Name = name;
        card.Age = xcard.aint("age", 1); // sollte eh immer 1 sein
        card.Image = card.dyn() ? Helpers.GetDynCardImage(civ) : Helpers.GetCivCardImage(name);
        card.X = xcard;
      }
      field.Card = card;

      card.CanPlace = true;//testing
      return card;

    }
    public static ACard MakeProgressCard(XElement xcard, AField field)
    {
      var name = xcard.astring("name");
      var type = xcard.astring("type");
      int age = xcard.aint("age", 1);
      var card = new ACard
      {
        Type = type,
        Brush = CT.typeColors[type],
        Name = name,
        Age = age,
        X = xcard,
        Image = Helpers.GetCardImage(name, age),
      };
      field.Card = card;

      card.CanBuy = true; //testing
      return card;
    }
    public static ACard MakeEventCard(XElement xcard)
    {
      var card = new ACard();
      card.Type = xcard.astring("type", "event");
      card.Brush = CT.typeColors[card.Type];
      card.Name = xcard.astring("name");
      card.Age = xcard.aint("age", 1);
      card.X = xcard;
      card.Image = Helpers.GetEventCardImage(card.Name, card.Age);
      return card;
    }

    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Name + "("+Type+")"; }
    #endregion
  }
}
