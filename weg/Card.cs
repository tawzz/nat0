using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ations
{
  public class Card : INotifyPropertyChanged  //TODO properties that change and are bound to need INotifyPropertyChanged
  {
    public string Type { get { return type; } set { if (type != value) { type = value; NotifyPropertyChanged(); } } }
    string type;
    public Brush Brush { get { return brush; } set { if (brush != value) { brush = value; NotifyPropertyChanged(); } } }
    Brush brush;
    public int Age { get { return age; } set { if (age != value) { age = value; NotifyPropertyChanged(); } } }
    int age;
    public BitmapImage Image { get { return image; } set { if (image != value) { image = value; NotifyPropertyChanged(); } } }
    BitmapImage image;
    public XElement X { get { return x; } set { if (x != value) { x = value; NotifyPropertyChanged(); } } }
    XElement x;


    //_______________________bis hier copiert (ohne NotifyProp...
    public Field Field { get { return field; } set { if (field != value) { field = value; NotifyPropertyChanged(); } } }
    Field field;
    public Brush SelectedBrush { get { return selectedBrush; } set { if (selectedBrush != value) { selectedBrush = value; NotifyPropertyChanged(); } } }
    Brush selectedBrush;
    public Thickness Margin { get { return margin; } set { if (margin != value) { margin = value; NotifyPropertyChanged(); } } }
    Thickness margin;
    public string Name { get { return name; } set { if (name != value) { name = value; NotifyPropertyChanged(); } } }
    string name;
    public bool IsEmpty { get { return isEmpty; } set { if (isEmpty != value) { isEmpty = value; NotifyPropertyChanged(); } } }
    bool isEmpty = true;
    public double Opacity { get { return opacity; } set { if (opacity != value) { opacity = value; NotifyPropertyChanged(); } } }
    double opacity;

    public int Cost { get; set; }
    public bool CanBuy { get { return canBuy; } set { if (canBuy != value) { canBuy = value; SelectedBrush = value?CardTypes.typeColors["selected"]:CardTypes.typeColors["normal"]; NotifyPropertyChanged();  } } }
    bool canBuy;
    public bool CanPlace { get { return canPlace; } set { if (canPlace != value) { canPlace = value; SelectedBrush = value ? CardTypes.typeColors["selected"] : CardTypes.typeColors["normal"]; NotifyPropertyChanged(); } } }
    bool canPlace;
    public int NumDeployed { get { return numDeployed; } set { if (numDeployed != value) { numDeployed = value; NotifyPropertyChanged(); } } }
    int numDeployed; //that includes architects and worker, even cross marks vielleicht mal sehen


    public static Card MakeInitialCivCard(Field f, string civ, string cname)
    {
      var card = new Card();
      card.IsEmpty = cname == "empty";
      //search for long info in either _commoncards.xml, [civ]cards.xml, or age 1 cards
      var xcard = Helpers.GetCivCardX(civ, cname)
        ?? Helpers.GetCommonCardX(cname)
        ?? Helpers.GetCardX(cname, 1);

      if (xcard != null)
      {
        card.Type = cname == "empty"?f.Type:xcard.astring("type");
        card.Field = f;
        card.Brush = CardTypes.typeColors[card.Type];
        card.Margin = f.Margin;
        card.Name = cname;
        card.Age = xcard.aint("age", 1); // sollte eh immer 1 sein
        card.Image = Helpers.GetCardImage("archimedes", card.Age); // eigentlich egal welche card
        card.Opacity = 0;// these cards are on the board
        card.X = xcard;
      }
      return card;
    }
    public static Card MakeProgressCard(XElement xcard, Field f)
    {
      var card = new Card();
      card.Type = xcard.astring("type");
      card.Field = f;
      card.Brush = CardTypes.typeColors[card.Type];
      card.Margin = f.Margin;
      card.Name = xcard.astring("name");
      card.Age = xcard.aint("age", 1);
      card.X = xcard;
      card.Image = Helpers.GetCardImage(card.Name, card.Age);
      card.Opacity = 1;
      return card;
    }
    public static void TurnIntoCivCard(Card newProgressCard, Card oldCivCard)
    {
      oldCivCard.Type = newProgressCard.Type;
      //oldCivCard.Field = f; stays
      oldCivCard.Brush = CardTypes.typeColors[newProgressCard.Type];
      //oldCivCard.Margin = f.Margin;
      oldCivCard.Name = newProgressCard.Name;
      oldCivCard.Age = newProgressCard.Age;
      oldCivCard.X = newProgressCard.X;
      oldCivCard.Image = newProgressCard.Image;
      oldCivCard.Opacity = 1;
    }
    public static Card MakeEventCard(XElement xcard)//, Thickness margin)
    {
      var card = new Card();
      card.Type = xcard.astring("type", "event");
      card.Brush = CardTypes.typeColors[card.Type];
      //card.M = margin;
      card.Name = xcard.astring("name");
      card.Age = xcard.aint("age", 1);
      card.X = xcard;
      card.Image = Helpers.GetEventCardImage(card.Name, card.Age);
      return card;
    }

    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString()
    {
      return Name + "(" + Type + ")";
    }
    //public static Card FromLonginfo(XElement x, Thickness m) // LongInfo is the xml that fully describes a card, found in assets/cards/xml
    //{
    //  return null;
    //}

    //public static Card FromAnyX(XElement x, Thickness m)
    //{ 
    //  if (x == null) return MakeUnknownCard();
    //  if (string.IsNullOrEmpty(x.astring("name"))) return MakeEmptyCard(x, m);//xel is a shortinfo for an empty field, still can get brush,margin,type
    //  if (x.aint("age",-1)==-1) //shortinfo has no age attribute
    //  {


    //  }
    //  return null;
    //}
    //public static Card MakeUnknownCard() { return new Card { Type = "unknown", Name = "unknown", Age = 1, Brush = Brushes.White, Margin = m }; }
    //public static Card MakeEmptyCard(XElement x,Thickness m) { return null; }

    //#endregion
    //public string Type { get; set; }
    //public string Name { get; set; }
    //public int Age { get; set; }
    //public XElement X { get; set; }


    //#region view
    //public Brush Brush { get; set; }
    //public Thickness Margin { get; set; }
    //public BitmapImage Image { get; set; }

    //#endregion
    // a dummy card has no name, it is basically an empty frame on the board
    //public static Card MakeDummyCard(Field f)
    //{
    //  return new Card
    //  {
    //    Type = f.Type,
    //    Field = f,
    //    Brush = CardTypes.typeColors[f.Type],
    //    Image = Helpers.GetCardImage("empty", 1),
    //    Opacity=0,
    //    Margin = f.Margin
    //  };
    //}
  }
}