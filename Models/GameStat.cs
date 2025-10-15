using SQLite;

namespace CatWorld.Models;

public class GameStat
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ToysCaught { get; set; }
    public int Score { get; set; }
    public long TotalPlaySeconds { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
