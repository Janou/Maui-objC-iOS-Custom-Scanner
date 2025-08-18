using ObjCRuntime;
using System.Runtime.InteropServices;

namespace Maui_objC_iOS_Custom_Scanner_Service.Platforms.iOS
{
    public static class ObjCRuntimeHelper
    {
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

        public static void SendMessage(IntPtr receiver, string selectorName, IntPtr arg1)
        {
            IntPtr selector = Selector.GetHandle(selectorName);
            objc_msgSend_void_IntPtr(receiver, selector, arg1);
        }
    }
}