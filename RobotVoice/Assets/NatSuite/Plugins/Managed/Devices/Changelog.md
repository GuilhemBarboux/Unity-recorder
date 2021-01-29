## NatDevice 1.0.2
+ Moved documentation [online](https://docs.natsuite.io/natdevice/).
+ Added native permissions requests on iOS and macOS.
+ Echo cancellation can now be enabled and disabled on `AudioDevice` instances that support it.
+ Changed `MediaDeviceQuery` to only accept a single criterion, instead of multiple.
+ Fixed hard crash on iPhone 6 when `MediaDeviceQuery` is created.
+ Fixed `AudioDevice` causing NatCorder to crash when recording is stopped on iOS.
+ Fixed `AudioDevice` reporting incorrect format before the device starts running on iOS.
+ Deprecated `MediaDeviceQuery.count` property. Use `MediaDeviceQuery.devices.Length` instead.
+ Deprecated `MediaDeviceQuery.Criterion` delegate type. Use `System.Predicate` delegate from .NET BCL instead.
+ Deprecated `MediaDeviceQuery.Criteria.EchoCancellation` criterion as it is no longer useful.

## NatDevice 1.0.1
+ Updated top-level namespace to `NatSuite.Devices` for parity with other NatSuite API's.
+ Fixed camera device query crash on Galaxy S10 and S10+.
+ Fixed sporadic crashes on some Android devices when the camera preview is started.
+ Fixed crash due to JNI local reference table overflow on Android.
+ Fixed `MediaDeviceQuery.Criteria.FrontFacing` not finding any cameras on iOS.
+ Fixed iOS archive generating error due to NatDevice not being built with full bitcode generation.

## NatDevice 1.0.0
+ First release.