using Plugin.Maui.Audio;

namespace CatWorld.Services;

public interface IAudioService
{
    bool IsEnabled { get; set; }

    // ����������� �������� ����
    Task PlayAsync(string file, double volume = 1.0);

    // ������� ������� �� �����
    Task PlayLoopAsync(string file, double volume = 0.5);

    // ���������� ���
    void StopLoop();
}

public class AudioService : IAudioService
{
    private readonly IAudioManager _audio;
    private IAudioPlayer? _loopPlayer;

    public AudioService(IAudioManager audio) => _audio = audio;

    public bool IsEnabled { get; set; } = true;

    public async Task PlayAsync(string file, double volume = 1.0)
    {
        if (!IsEnabled) return;

        using var stream = await FileSystem.OpenAppPackageFileAsync(file);
        using var player = _audio.CreatePlayer(stream);
        player.Volume = Math.Clamp(volume, 0, 1);
        player.Play();
        // ������� ���������� (����� �������� �����)
        await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(50, player.Duration * 1000)));
    }

    public async Task PlayLoopAsync(string file, double volume = 0.5)
    {
        StopLoop();                    // ������ ���������� ��� ���� ���
        if (!IsEnabled) return;

        var stream = await FileSystem.OpenAppPackageFileAsync(file);
        _loopPlayer = _audio.CreatePlayer(stream);
        _loopPlayer.Loop = true;
        _loopPlayer.Volume = Math.Clamp(volume, 0, 1);
        _loopPlayer.Play();
    }

    public void StopLoop()
    {
        try
        {
            if (_loopPlayer is null) return;
            _loopPlayer.Stop();
            _loopPlayer.Dispose();
            _loopPlayer = null;
        }
        catch { /* ignore */ }
    }
}
