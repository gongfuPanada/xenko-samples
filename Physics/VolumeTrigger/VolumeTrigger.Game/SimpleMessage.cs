using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeTrigger
{
    public class SimpleMessage
    {
        public static event EventHandler Start;
        public static event EventHandler Stop;

        public static void OnStart()
        {
            Start?.Invoke(null, EventArgs.Empty);
        }

        public static void OnStop()
        {
            Stop?.Invoke(null, EventArgs.Empty);
        }
    }
}
