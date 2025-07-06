using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers;

public enum GameMode
{
    EDITOR,GAME
}
public class ConfigManager: SingletonBase<ConfigManager>
{
    private GameMode gameMode = GameMode.EDITOR;

    public GameMode GameMode { get => gameMode; }

    public void SetGameMode(GameMode gameMode)
    {
        this.gameMode = gameMode;

        switch (gameMode)
        {
            case GameMode.EDITOR:
                ConfigEditor();

                break;
            case GameMode.GAME:
                ConfigGame();
                break;
            default:
                break;
        }
    }

    private void ConfigGame()
    {
        throw new NotImplementedException();
    }

    private void ConfigEditor()
    {
        throw new NotImplementedException();
    }
}
