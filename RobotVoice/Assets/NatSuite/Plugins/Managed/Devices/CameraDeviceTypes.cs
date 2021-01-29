/* 
*   NatDevice
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    /// <summary>
    /// Photo flash mode.
    /// </summary>
    public enum FlashMode : int {
        /// <summary>
        /// Never use flash.
        /// </summary>
        Off = 0,
        /// <summary>
        /// Always use flash.
        /// </summary>
        On = 1,
        /// <summary>
        /// Let the sensor detect if it needs flash.
        /// </summary>
        Auto = 2
    }

    /// <summary>
    /// Frame orientation.
    /// </summary>
    public enum FrameOrientation : int {
        /// <summary>
        /// Landscape left.
        /// </summary>
        LandscapeLeft = 0,
        /// <summary>
        /// Portrait.
        /// </summary>
        Portrait = 1,
        /// <summary>
        /// Landscape right.
        /// </summary>
        LandscapeRight = 2,
        /// <summary>
        /// Portrait upside down.
        /// </summary>
        PortraitUpsideDown = 3
    }
}