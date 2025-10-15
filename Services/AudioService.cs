using Plugin.Maui.Audio;

namespace CatWorld.Services;

public interface IAudioService
{
    Task PlayAsync(string fileName);
    Task StopAsync();
    bool IsEnabled { get; set; }
}

public class AudioService : IAudioService
{
    private readonly IAudioManager _audio;
    private IAudioPlayer? _player;
    public bool IsEnabled { get; set; } = true;

    public AudioService(IAudioManager audio) => _audio = audio;

    public async Task PlayAsync(string fileName)
    {
        if (!IsEnabled) return;
        _player?.Stop();
        using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        _player = _audio.CreatePlayer(stream);
        _player.Play();
    }

    public Task StopAsync()
    {
        _player?.Stop();
        return Task.CompletedTask;
    }
}
