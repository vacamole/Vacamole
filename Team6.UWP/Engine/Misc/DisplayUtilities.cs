using System;
using System.Collections.Generic;
#if LINUX
using System.Drawing;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public class DisplayUtilities : EngineComponent
    {
        public DisplayUtilities(MainGame game) : base(game)
        {
            RawDpiX = 96;
            RawDpiY = 96;
            ScalingFactor = 1;

            InitDpi();

        }

        public event Action DpiChanged;

        public float ScalingFactor { get; private set; }
        public float RawDpiX { get; private set; }
        public float RawDpiY { get; private set; }

#if WINDOWS_UAP
        private void InitDpi()
        {
            var displayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
            UpdateDpi();
            displayInformation.DpiChanged += DisplayInformation_DpiChanged;
        }

        private void DisplayInformation_DpiChanged(Windows.Graphics.Display.DisplayInformation sender, object args)
        {
            UpdateDpi();
        }

        private void UpdateDpi()
        {
            var displayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
            ScalingFactor = displayInformation.LogicalDpi / 96f;
            RawDpiX = displayInformation.RawDpiX;
            RawDpiY = displayInformation.RawDpiY;

            DpiChanged?.Invoke();
        }
#else
        private void InitDpi()
        {
            try
            {
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    RawDpiX = graphics.DpiX;
                    RawDpiY = graphics.DpiY;
                    ScalingFactor = RawDpiX / 96f;
                    DpiChanged?.Invoke();
                }
            }
            catch
            {
                // Just use the default DPI
            }
        }
#endif
    }
}
