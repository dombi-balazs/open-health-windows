using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_health_windows.Entities
{
    class ChosenHardwareEntity
    {
        public enum HardwareChoice
        {
            CPU,
            NPU
        }
        public HardwareChoice ChosenHardware { get; set; } = HardwareChoice.CPU;
    }
}
