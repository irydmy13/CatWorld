namespace CatWorld.Models;

public class CatState
{
    public double X { get; set; }           // позиция кота на экране
    public double Y { get; set; }
    public int Happiness { get; set; } = 50; // 0..100
    public int Energy { get; set; } = 80;    // 0..100
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;
}
