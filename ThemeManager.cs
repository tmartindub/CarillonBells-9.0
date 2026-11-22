using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace Carillon;
public sealed class IntBoolConverter : IValueConverter
{
    public object Convert(object value,Type targetType,object parameter,CultureInfo culture)=>value is not DBNull&&System.Convert.ToInt32(value)!=0;
    public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture)=>value is true?1:0;
}
public sealed class GroupDurationConverter : IValueConverter
{
    public object Convert(object value,Type targetType,object parameter,CultureInfo culture)=>value is System.Collections.IEnumerable items?"  ·  "+DataFormats.DurationText(TimeSpan.FromTicks(items.Cast<object>().OfType<Song>().Sum(s=>DataFormats.Duration(s.Duration).Ticks))):"";
    public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture)=>Binding.DoNothing;
}
public sealed class CarillonPalette
{
    public string Name { get; set; } = "";
    public string Accent { get; set; } = "#D87732";
    public string Canvas { get; set; } = "#EFF3F6";
    public string Ink { get; set; } = "#173650";
    public string Surface { get; set; } = "#FFFFFF";
    public string? CardSurface { get; set; }
}
public static class ThemeManager
{
    public static CarillonPalette CurrentPalette { get; private set; } = new() {Name="Porcelain Blue"};
    private static Color Parse(string text)
    {
        if(int.TryParse(text,out var v)) return Color.FromRgb((byte)v,(byte)(v>>8),(byte)(v>>16));
        return (Color)ColorConverter.ConvertFromString(text);
    }
    public static void Apply(CarillonPalette palette,CarillonRepository? repository=null)
    {
        CurrentPalette=palette;
        var resources=Application.Current.Resources;
        foreach(var (key,value) in new[]{("Canvas",palette.Canvas),("Ink",palette.Ink),("Surface",palette.Surface),("Accent",palette.Accent)}) resources[key]=new SolidColorBrush(Parse(value));
        var canvas=Parse(palette.Canvas);
        // Custom themes store the canvas and text colors. Give their cards a subtle shade of the canvas.
        var shade=0.2126*canvas.R+0.7152*canvas.G+0.0722*canvas.B<128?255:0;
        var cardColor=palette.CardSurface is { } tint
            ? Parse(tint)
            : Color.FromArgb(canvas.A,(byte)((canvas.R*9+shade)/10),(byte)((canvas.G*9+shade)/10),(byte)((canvas.B*9+shade)/10));
        resources["CardSurface"]=new SolidColorBrush(cardColor);
        resources["Muted"]=new SolidColorBrush(Parse(palette.Ink));
        resources[SystemColors.WindowBrushKey]=new SolidColorBrush(Parse(palette.Surface));resources[SystemColors.WindowTextBrushKey]=new SolidColorBrush(Parse(palette.Ink));resources[SystemColors.ControlBrushKey]=new SolidColorBrush(Parse(palette.Surface));resources[SystemColors.ControlTextBrushKey]=new SolidColorBrush(Parse(palette.Ink));
        if(repository is not null) { var values=repository.Settings(); values["form_bgcolor"]=Vcl(Parse(palette.Canvas)); values["form_fontcolor"]=Vcl(Parse(palette.Ink)); repository.SaveSettings(values,["form_bgcolor","form_fontcolor"]); }
    }
    private static string Vcl(Color color)=>(color.R|(color.G<<8)|(color.B<<16)).ToString(CultureInfo.InvariantCulture);
    public static void Load(CarillonRepository repository)
    {
        var values=repository.Settings(); var background=values["form_bgcolor"].ToString(); var text=values["form_fontcolor"].ToString();
        if(string.IsNullOrWhiteSpace(background)||string.IsNullOrWhiteSpace(text)) return;
        var catalog=(CarillonPalette[])Application.Current.FindResource("ThemeCatalog");
        var palette=catalog.FirstOrDefault(p=>Parse(p.Canvas)==Parse(background)&&Parse(p.Ink)==Parse(text))??new CarillonPalette { Name="Current: Custom Saved Colors",Canvas=background,Ink=text,Surface=background };
        Apply(palette);
    }
}
