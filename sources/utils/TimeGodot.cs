using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class TimeGodot
 {
    public static float Delta { get; private set; }
    public static float PhysicsDelta { get; private set; }
    
    public static void UpdateDelta(float newDelta) => Delta = newDelta;
    public static void UpdatePhysicsDelta(float newDelta) => PhysicsDelta = newDelta;
}

