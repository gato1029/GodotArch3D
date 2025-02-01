using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils
{
    public interface IDetailWindow
    {        
        public delegate void RequestUpdateHandler();
        public event RequestUpdateHandler OnRequestUpdate;
        public void LoadData(int id);
    }
    public enum WindowState
    {
       NEW,UPDATE,CRUD,SELECTOR
    }
    class WindowHelper
    {
    }
}
