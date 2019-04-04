using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Team6.Engine.Graphics2d
{
    public class AnimationDefintion
    {
        public string AssetName { get; set; }
        public int TilingX { get; set; }
        public int TilingY { get; set; }
        public int[] Neutral { get; set; }
        public List<Animation> Animations { get; set; }

        public class Animation
        {
            private int[] frames;
            public string Name { get; set; }
            public float Duration { get; set; }
            public string Frames { get { return RenderFrames(frames); } set { ParseFrames(value); } }
            public float EntitySpeedScale { get; set; } = -1f;

            public float EntitySpeedOffset { get; set; } = 0f;
            [JsonIgnore]
            public bool IsScaledWithEntity { get { return EntitySpeedScale > 0f; } }
            [JsonIgnore]
            public int[] FrameNumbers { get { return frames; } }
            private string RenderFrames(int[] frames)
            {
                return string.Join(",", frames);
            }
            private void ParseFrames(string value)
            {
                List<int> frames = new List<int>();

                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE (By splitting)
                foreach (string segment in value.Split(new char[] { ',', ';' }))
                {
                    int start, end;
                    if (segment.Contains('-'))
                    {
                        var startAndEnd = segment.Split('-');

                        if (startAndEnd.Length != 2)
                            throw new ArgumentException(nameof(value));

                        if (!int.TryParse(startAndEnd[0], out start) || !int.TryParse(startAndEnd[1], out end))
                            throw new ArgumentException(nameof(value));

                        for (int i = start; i <= end; i++)
                            frames.Add(i);
                    }
                    else
                    {
                        if (!int.TryParse(segment, out start))
                            throw new ArgumentException(nameof(value));
                        frames.Add(start);
                    }
                }

                this.frames = frames.ToArray();
            }
        }
    }
}
