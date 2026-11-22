using System.Windows.Media;

namespace Carillon;

public sealed class ClockHandStyle
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DrawingBrush Hour { get; set; } = new();
    public DrawingBrush Minute { get; set; } = new();
    public DrawingBrush Cap { get; set; } = new();
}
