namespace CatWorld.Services;

public interface INightModeService
{
    /// <summary>Сейчас ночь по расписанию?</summary>
    bool IsNightNow();
}
