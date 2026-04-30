using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;

public static class ModHelper
{
    public static InfoModData Mod;
    public static void Init()
    {
        DataBaseManager.Instance.LoadCurrentDataBase();
        Mod =DataBaseManager.Instance.FindById<InfoModData>(1);
        int a = 0;
    }
}
