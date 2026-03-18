namespace open_health_windows.Entities
{
    class ChosenHardwareEntity
    {
        public enum HardwareChoice
        {
            CPU,
            iGPU,
            NPU
        }
        public HardwareChoice ChosenHardware { get; set; } = HardwareChoice.CPU;
    }
}