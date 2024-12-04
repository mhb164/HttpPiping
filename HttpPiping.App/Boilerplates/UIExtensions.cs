using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://github.com/mhb164/Boilerplates
namespace Boilerplates
{
    public static class UIExtensions
    {
        public static bool IsRuntime(this Form item) => LicenseManager.UsageMode == LicenseUsageMode.Runtime;
        public static bool IsRuntime(this Control item) => LicenseManager.UsageMode == LicenseUsageMode.Runtime;
        public static bool IsRuntime(this UserControl item) => LicenseManager.UsageMode == LicenseUsageMode.Runtime;

        public static bool IsDesigntime(this Form item) => LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        public static bool IsDesigntime(this Control item) => LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        public static bool IsDesigntime(this UserControl item) => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public static void InvokeIfNecessary(this Form item, MethodInvoker action) { if (item?.InvokeRequired == true) item?.BeginInvoke(action); else { action(); } }
        public static void InvokeIfNecessary(this Control item, MethodInvoker action) { if (item?.InvokeRequired == true) item?.BeginInvoke(action); else { action(); } }
        public static void InvokeIfNecessary(this UserControl item, MethodInvoker action) { if (item?.InvokeRequired == true) item?.BeginInvoke(action); else { action(); } }
    }
}

//namespace HttpPiping.App.Boilerplates
//{
//    public static partial class UIExtensions
//    {
//        public static void InvokeIfRequired(this ISynchronizeInvoke item, MethodInvoker action)
//        {
//            if (!item.InvokeRequired)
//            {
//                action();
//                return;
//            }

//            item.Invoke(action, null);
//        }

//        public static void InvokeIfRequired(this ISynchronizeInvoke item, Action action)
//        {
//            if (!item.InvokeRequired)
//            {
//                action();
//                return;
//            }

//            item.Invoke(action, null);
//        }

//        public static T InvokeIfRequired<T>(this ISynchronizeInvoke item, Func<T> function)
//        {
//            if (!item.InvokeRequired)
//            {
//                return function();
//            }

//            return (T)item.Invoke(function, null);
//        }

//        public static void InvokeIfNecessary(this Form item, MethodInvoker action)
//            => (item as ISynchronizeInvoke).InvokeIfRequired(action);

//        public static void InvokeIfNecessary(this Control item, MethodInvoker action)
//            => (item as ISynchronizeInvoke).InvokeIfRequired(action);

//        public static void InvokeIfNecessary(this UserControl item, MethodInvoker action)
//            => (item as ISynchronizeInvoke).InvokeIfRequired(action);

//    }
//}
