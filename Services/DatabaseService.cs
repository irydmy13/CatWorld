using SQLite;
using CatWorld.Models;

namespace CatWorld.Services;

public interface IDatabaseService
{
    Task InitAsync();
    Task<int> SaveToyAsync(Toy toy);
    Task<List<Toy>> GetToysAsync();
    Task<int> SaveStatAsync(GameStat stat);
    Task<GameStat> GetStatAsync();
}

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection _db;
    private const string DbName = "catworld.db3";

    public async Task InitAsync()
    {
        if (_db != null) return;
        var path = Path.Combine(FileSystem.AppDataDirectory, DbName);
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<Toy>();
        await _db.CreateTableAsync<GameStat>();

        // seed игрушки при первом запуске
        if ((await _db.Table<Toy>().CountAsync()) == 0)
        {
            await _db.InsertAllAsync(new[]
            {
                new Toy { Name = "Ball", Icon="toy_ball.png", Fun=10 },
                new Toy { Name = "Mouse", Icon="toy_mouse.png", Fun=15 },
            });
        }
        if ((await _db.Table<GameStat>().CountAsync()) == 0)
            await _db.InsertAsync(new GameStat());
    }

    public Task<int> SaveToyAsync(Toy toy) => _db.InsertOrReplaceAsync(toy);
    public Task<List<Toy>> GetToysAsync() => _db.Table<Toy>().ToListAsync();

    public async Task<int> SaveStatAsync(GameStat stat)
    {
        if (stat.Id == 0) return await _db.InsertAsync(stat);
        return await _db.UpdateAsync(stat);
    }

    public async Task<GameStat> GetStatAsync()
    {
        var all = await _db.Table<GameStat>().ToListAsync();
        return all.FirstOrDefault() ?? new GameStat();
    }
}
