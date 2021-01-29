/* 
*   NatDevice
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    using AOT;
    using UnityEngine;
    using UnityEngine.Android;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Internal;

    public sealed partial class MediaDeviceQuery {
        
        #region --Client API--
        /// <summary>
        /// Reuest permissions to use media devices from the user.
        /// </summary>
        public static Task<bool> RequestPermissions<T> () where T : IMediaDevice {
            // Get type
            var permissionType = (PermissionType)0;
            if (typeof(ICameraDevice).IsAssignableFrom(typeof(T)))
                permissionType = PermissionType.Camera;
            else if (typeof(IAudioDevice).IsAssignableFrom(typeof(T)))
                permissionType = PermissionType.Audio;
            // Check
            if (permissionType == 0)
                return Task.FromResult(true);
            // Request
            var permissionTask = new TaskCompletionSource<bool>();
            if (Application.platform == RuntimePlatform.Android) {
                var requester = new GameObject("MediaDeviceQuery Permissions Helper").AddComponent<MediaDeviceQueryPermissionsHelper>();
                requester.StartCoroutine(RequestAndroid(requester));
            } else {
                var handle = GCHandle.Alloc(permissionTask, GCHandleType.Normal);
                RequestPermissions(permissionType, OnPermissionResult, (IntPtr)handle);
            }
            return permissionTask.Task;
            // Define Android request
            IEnumerator RequestAndroid (MediaDeviceQueryPermissionsHelper requester) {
                var permission = permissionType == PermissionType.Camera ? Permission.Camera : Permission.Microphone;
                if (!Permission.HasUserAuthorizedPermission(permission))
                    Permission.RequestUserPermission(permission);
                yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(permission));
                permissionTask.SetResult(true);
                MonoBehaviour.Destroy(requester.gameObject);
            }
        }
        #endregion


        #region --Operations--

        private enum PermissionType { Audio = 1, Camera = 2 } // CHECK // Must match definition in `NatDevice.h`

        private delegate void PermissionResultHandler (IntPtr context, bool granted);

        #if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport(Bridge.Assembly, EntryPoint = @"NDRequestPermissions")]
        private static extern void RequestPermissions (PermissionType permissionType, PermissionResultHandler handler, IntPtr context);
        #else
        private static void RequestPermissions (PermissionType permissionType, PermissionResultHandler handler, IntPtr context) { }
        #endif

        [MonoPInvokeCallback(typeof(PermissionResultHandler))]
        private static void OnPermissionResult (IntPtr context, bool granted) {
            var handle = (GCHandle)context;
            var permissionTask = handle.Target as TaskCompletionSource<bool>;
            handle.Free();
            permissionTask.SetResult(granted);
        }

        private sealed class MediaDeviceQueryPermissionsHelper : MonoBehaviour {
            void Awake () => DontDestroyOnLoad(this.gameObject);
        }
        #endregion
    }
}