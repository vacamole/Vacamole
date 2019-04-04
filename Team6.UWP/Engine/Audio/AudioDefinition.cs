namespace Team6.Engine.Audio
{
    public class AudioDefinition
    {
        public float MinPitchShift { get; set; } = 0;
        public float MaxPitchShift { get; set; } = 0;
        public float MinVolume { get; set; } = 1;
        public float MaxVolume { get; set; } = 1;
        public int InstanceLimit { get; set; } = 5;
        public float MinimumTimeBetween { get; set; } = 0;
    }
}
