using SQLite;

namespace CatWorld.Models;

public class Toy
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Icon { get; set; } = "toy_ball.png"; // путь к картинке
    public int Fun { get; set; } = 10;                 // сколько радости даёт
}
