/* 
*   NatDevice
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    using System;

    public sealed partial class MediaDeviceQuery {

        /// <summary>
        /// Common criteria used to filter devices.
        /// </summary>
        public static class Criteria {

            /// <summary>
            /// Filter for hardware audio devices.
            /// </summary>
            public static readonly Predicate<IMediaDevice> AudioDevice = device => device is AudioDevice;

            /// <summary>
            /// Filter for hardware camera devices.
            /// </summary>
            public static readonly Predicate<IMediaDevice> CameraDevice = device => device is CameraDevice;

            /// <summary>
            /// Filter for generic camera devices.
            /// This criterion will filter for either `CameraDevice` or `WebCameraDevice` instances.
            /// </summary>
            public static readonly Predicate<IMediaDevice> GenericCameraDevice = device => device is ICameraDevice;

            /// <summary>
            /// Filter for rear-facing camera devices.
            /// </summary>
            public static readonly Predicate<IMediaDevice> RearFacing = device => device is ICameraDevice camera && !camera.frontFacing;

            /// <summary>
            /// Filter for front-facing camera devices.
            /// </summary>
            public static readonly Predicate<IMediaDevice> FrontFacing = device => device is ICameraDevice camera && camera.frontFacing;
            
            /// <summary>
            /// Filter for camera devices that have torches.
            /// </summary>
            public static readonly Predicate<IMediaDevice> Torch = device => device is CameraDevice camera && camera.torchSupported;
        }
    }
}