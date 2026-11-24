using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using System.IO;
using System.Text.Json;
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
    public static string CurrentFontFamily { get; private set; }="Original";
    public static double CurrentFontSize { get; private set; }=18;
    private static readonly string[] FontFamilies=["Original","Bahnschrift","Segoe UI","Arial","Tahoma","Verdana","Calibri"];
    private static readonly int[] FontResourceSizes=[14,15,16,18,19,22,25,28,30,32];
    internal static void SetFontResources(ResourceDictionary resources,string family,double size)
    {
        if(!FontFamilies.Contains(family)||size<14||size>20||size!=Math.Truncate(size)) throw new ArgumentException("Choose a listed font and a whole-number size from 14 through 20.");
        if(family!="Original"&&!Fonts.SystemFontFamilies.Any(font=>font.Source.Equals(family,StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("This font is not installed on this computer.");
        resources["BodyFontFamily"]=new FontFamily(family=="Original"?"Bahnschrift":family);
        resources["HeadingFontFamily"]=new FontFamily(family=="Original"?"Georgia":family);
        foreach(var original in FontResourceSizes) resources["UiFontSize"+original]=original*size/18;
    }
    internal static IEnumerable<FrameworkElement> FontElements(DependencyObject root)
    {
        for(var index=0;index<VisualTreeHelper.GetChildrenCount(root);index++)
        {
            var child=VisualTreeHelper.GetChild(root,index);
            if(child is FrameworkElement element) yield return element;
            foreach(var descendant in FontElements(child)) yield return descendant;
        }
    }
    internal static List<string> CheckFontLayout(FrameworkElement root,string page,IReadOnlyList<Song>? songSamples=null)
    {
        var failures=new HashSet<string>();
        foreach(var element in FontElements(root))
        {
            if(element.Visibility!=Visibility.Visible||element.ActualWidth<=1||element.ActualHeight<=1) continue;
            string text; FontFamily family; FontStyle style; FontWeight weight; FontStretch stretch; double size,width,height; bool wrap=false;
            if(element is TextBox box)
            {
                if(box.IsReadOnly&&box.AcceptsReturn) continue; // Report viewers retain vertical paging.
                text=box.Text;family=box.FontFamily;style=box.FontStyle;weight=box.FontWeight;stretch=box.FontStretch;size=box.FontSize;
                width=box.ActualWidth-box.Padding.Left-box.Padding.Right-box.BorderThickness.Left-box.BorderThickness.Right-2;
                height=box.ActualHeight-box.Padding.Top-box.Padding.Bottom-box.BorderThickness.Top-box.BorderThickness.Bottom;
                wrap=box.TextWrapping!=TextWrapping.NoWrap;
            }
            else if(element is TextBlock block)
            {
                text=block.Text;family=block.FontFamily;style=block.FontStyle;weight=block.FontWeight;stretch=block.FontStretch;size=block.FontSize;
                width=block.ActualWidth-block.Padding.Left-block.Padding.Right;
                height=block.ActualHeight-block.Padding.Top-block.Padding.Bottom;
                wrap=block.TextWrapping!=TextWrapping.NoWrap;
                if(VisualTreeHelper.GetParent(block) is Border border) {width=Math.Min(width,border.ActualWidth-border.Padding.Left-border.Padding.Right-border.BorderThickness.Left-border.BorderThickness.Right);height=Math.Min(height,border.ActualHeight-border.Padding.Top-border.Padding.Bottom-border.BorderThickness.Top-border.BorderThickness.Bottom);}
                if(VisualTreeHelper.GetParent(block) is ContentPresenter presenter && presenter.ActualWidth>1 && presenter.ActualHeight>1) {width=Math.Min(width,presenter.ActualWidth);height=Math.Min(height,presenter.ActualHeight);}
            }
            else if(element is PasswordBox password)
            {
                text=new string('●',password.Password.Length); family=password.FontFamily;style=password.FontStyle;weight=password.FontWeight;stretch=password.FontStretch;size=password.FontSize;
                width=password.ActualWidth-password.Padding.Left-password.Padding.Right-4;height=password.ActualHeight-password.Padding.Top-password.Padding.Bottom-2;
            }
            else continue;
            IEnumerable<string> values=[text];
            if(songSamples is not null&&element is TextBox field)
            {
                values=field.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.Path?.Path switch
                {
                    "EditingSong.Group"=>songSamples.Select(song=>song.Group),
                    "EditingSong.Repeats"=>songSamples.Select(song=>song.Repeats.ToString(CultureInfo.InvariantCulture)),
                    "Value" when field.DataContext is PlayTime=>songSamples.SelectMany(song=>song.Times).Select(time=>time.Value),
                    _=>values
                };
                if(field.TemplatedParent is DatePicker datePicker)
                {
                    var binding=datePicker.GetBindingExpression(DatePicker.SelectedDateProperty)?.ParentBinding.Path?.Path;
                    if(binding is "EditingSong.From" or "EditingSong.To") values=songSamples.Select(song=>(binding=="EditingSong.From"?song.From:song.To)?.ToString("MM/dd/yyyy",CultureInfo.InvariantCulture)??"");
                }
            }
            foreach(var value in values.Where(value=>!string.IsNullOrEmpty(value)).Distinct())
            {
                var measured=new FormattedText(value,CultureInfo.GetCultureInfo("en-US"),element.FlowDirection,new Typeface(family,style,weight,stretch),size,Brushes.Black,VisualTreeHelper.GetDpi(Application.Current.MainWindow??root).PixelsPerDip);
                if(wrap) measured.MaxTextWidth=Math.Max(1,width);
                if((wrap?measured.Width:measured.WidthIncludingTrailingWhitespace)<=width+1&&measured.Height<=height+1) continue;
                var label=AutomationProperties.GetName(element);
                if(label.Length==0&&element is TextBox input) label=input.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.Path?.Path??input.Name;
                if(label.Length==0&&element is TextBlock caption) label=caption.GetBindingExpression(TextBlock.TextProperty) is null?caption.Text:caption.GetBindingExpression(TextBlock.TextProperty)!.ParentBinding.Path?.Path??"display text";
                if(element.TemplatedParent is Button button) label=button.Content+" button";
                label=label switch {"[SEASONALGROUP]"=>"Group name","[rand_music_dir]"=>"Music directory","[frm_date]"=>"From date","[to_date]"=>"Until date","[Purpose]"=>"Silence purpose","Title" or "SelectedSong.Title"=>"Song name","EditingSong.Group"=>"Song group","PlayerTitle"=>"Last song played","TodayLabel"=>"Date/time line","Organization"=>"Organization name","Name" when page=="Clock hands"=>"Clock-hand style name","Name"=>"Group name","Event"=>"Log entry",_=>label};
                if(label.Length==0) label=element is PasswordBox?"password field":"text field";
                failures.Add($"{page}: {label} is too {((wrap?measured.Width:measured.WidthIncludingTrailingWhitespace)>width+1?"wide":"tall")} for its space. Choose a smaller size or a narrower font.");
            }
        }
        return failures.ToList();
    }
    public static List<string> CheckFonts(CarillonRuntime runtime,string family,double size)
    {
        var failures=new HashSet<string>();
        if(SystemParameters.WorkArea.Width<1200||SystemParameters.WorkArea.Height<740)
            return ["The Windows work area is smaller than the app's minimum 1200 × 740 window. Check the display resolution and scaling before changing fonts."];
        void Review(Window window,string name,double width,double height,IReadOnlyList<Song>? songSamples=null)
        {
            SetFontResources(window.Resources,family,size);
            var content=(FrameworkElement)window.Content;
            content.Measure(new Size(width,height));content.Arrange(new Rect(0,0,width,height));content.UpdateLayout();
            content.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ContextIdle);
            content.InvalidateMeasure();content.Measure(new Size(width,height));content.Arrange(new Rect(0,0,width,height));content.UpdateLayout();
            foreach(var failure in CheckFontLayout(content,name,songSamples)) failures.Add(failure);
            foreach(var picker in FontElements(content).OfType<DatePicker>())
            {
                if(picker.Template.FindName("PART_Popup",picker) is not Popup {Child:FrameworkElement calendar}) continue;
                SetFontResources(calendar.Resources,family,size);
                calendar.Measure(new Size(400,400));calendar.Arrange(new Rect(new Point(),calendar.DesiredSize));calendar.UpdateLayout();
                foreach(var failure in CheckFontLayout(calendar,name+" calendar")) failures.Add(failure);
            }
        }
        var main=new MainWindow(runtime,appearancePreview:true);
        var admin=new AdministrationWindow(runtime,"Fonts",appearancePreview:true);
        var log=new LogWindow(runtime.Log,appearancePreview:true);
        var clock=new ClockWindow();
        var splash=new SplashWindow();
        try
        {
            var sizes=new[]{new Size(1184,701),new Size(Math.Max(1184,SystemParameters.WorkArea.Width-16),Math.Max(701,SystemParameters.WorkArea.Height-39))}.Distinct();
            foreach(var screen in sizes)
            {
                Review(main,"Playlist",screen.Width,screen.Height,runtime.Songs);
                var titleDisplay=FontElements((FrameworkElement)main.Content).OfType<TextBlock>().FirstOrDefault(block=>block.GetBindingExpression(TextBlock.TextProperty)?.ParentBinding.Path?.Path=="SelectedSong.Title");
                if(titleDisplay is not null&&main.Songs.Count>0)
                {
                    main.SelectedSong=main.Songs.MaxBy(song=>
                    {
                        var title=new FormattedText(song.Title,CultureInfo.GetCultureInfo("en-US"),titleDisplay.FlowDirection,new Typeface(titleDisplay.FontFamily,titleDisplay.FontStyle,titleDisplay.FontWeight,titleDisplay.FontStretch),titleDisplay.FontSize,Brushes.Black,VisualTreeHelper.GetDpi(Application.Current.MainWindow??main).PixelsPerDip);
                        title.MaxTextWidth=Math.Max(1,titleDisplay.ActualWidth);return title.Height;
                    });
                    Review(main,"Playlist",screen.Width,screen.Height,runtime.Songs);
                }
                main.ScheduleClick(main,new RoutedEventArgs());
                Review(main,"Today's schedule",screen.Width,screen.Height);
                main.ScheduleClick(main,new RoutedEventArgs());
                var adminWidth=Math.Max(1084,screen.Width-100);var adminHeight=Math.Max(581,screen.Height-80);
                foreach(TabItem tab in ((TabControl)admin.FindName("Pages")).Items)
                {
                    ((TabControl)admin.FindName("Pages")).SelectedItem=tab;
                    Review(admin,tab.Header.ToString()!,adminWidth,adminHeight);
                    if(tab.Tag?.ToString()=="Groups"&&admin.Groups.GetChanges() is not null) throw new InvalidOperationException("The group date preview created pending edits; font changes were not applied.");
                    if(tab.Tag?.ToString()=="Email")
                    {
                        var show=(CheckBox)admin.FindName("ShowPassword");show.IsChecked=true;admin.ShowPasswordClick(show,new RoutedEventArgs());
                        Review(admin,"Email / reports (password visible)",adminWidth,adminHeight);
                        show.IsChecked=false;admin.ShowPasswordClick(show,new RoutedEventArgs());
                    }
                }
                Review(log,"Log",screen.Width,screen.Height);
                Review(clock,"Clock menu",screen.Width,screen.Height);
            }
            Review(splash,"Startup",splash.Width,splash.Height);
        }
        finally {main.Close();admin.Close();log.Close();clock.Close();splash.Close();}
        return failures.ToList();
    }
    public static void SaveFonts(string baseDirectory,string family,double size)
    {
        // Validate before writing; the caller has also checked every page using current data.
        var candidate=new ResourceDictionary();SetFontResources(candidate,family,size);
        var path=Path.Combine(baseDirectory,"data","fonts.json");Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary=path+".tmp";
        try {File.WriteAllText(temporary,JsonSerializer.Serialize(new {Family=family,Size=size}));File.Move(temporary,path,true);}
        finally {if(File.Exists(temporary))File.Delete(temporary);}
        SetFontResources(Application.Current.Resources,family,size);CurrentFontFamily=family;CurrentFontSize=size;
    }
    public static string? LoadFonts(CarillonRuntime runtime)
    {
        var path=Path.Combine(runtime.BaseDirectory,"data","fonts.json");
        if(!File.Exists(path)) return null;
        try
        {
            using var saved=JsonDocument.Parse(File.ReadAllText(path));var family=saved.RootElement.GetProperty("Family").GetString()??"Original";var size=saved.RootElement.GetProperty("Size").GetDouble();
            if(family=="Original"&&size==18) {SetFontResources(Application.Current.Resources,"Original",18);CurrentFontFamily="Original";CurrentFontSize=18;return null;}
            var failures=CheckFonts(runtime,family,size);
            if(failures.Count>0) {SetFontResources(Application.Current.Resources,"Original",18);CurrentFontFamily="Original";CurrentFontSize=18;return "Saved font did not pass this computer's layout check. Original appearance restored. Open System → Font settings to review.\n"+failures[0];}
            SetFontResources(Application.Current.Resources,family,size);CurrentFontFamily=family;CurrentFontSize=size;
            return null;
        }
        catch(Exception error) when(error is IOException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException or KeyNotFoundException)
        {SetFontResources(Application.Current.Resources,"Original",18);CurrentFontFamily="Original";CurrentFontSize=18;return "Saved font could not be used. Original appearance restored. "+error.Message;}
    }
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
