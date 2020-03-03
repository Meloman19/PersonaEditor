using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    sealed class AnyEventHandler : IDisposable
    {
        public event Action<object[]> EventRaise;

        GCHandle GCHandle;
        Delegate dynamicMethodDelegate = null;
        EventInfo eventInfo = null;
        object source = null;

        public AnyEventHandler(object source, string eventName)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentException("eventName is null or empty or contain only spaces", "eventName");

            CreateHandler(source, eventName);
        }

        private bool CreateHandler(object source, string eventName)
        {
            eventInfo = source.GetType().GetEvent(eventName);
            if (eventInfo == null)
                return false;
            var handlerType = eventInfo.EventHandlerType;
            var handlerTypeArray = handlerType.GetMethod("Invoke")?.GetParameters().Select(x => x.ParameterType).ToArray();

            GCHandle = GCHandle.Alloc(new Action<object[]>(EventInvoke));

            var handlerPtr64 = GCHandle.ToIntPtr(GCHandle).ToInt64();
            var methodInfo = GetType().GetMethod("Invoke", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            DynamicMethod dynamicMethod = new DynamicMethod("EventGetArg", typeof(void), handlerTypeArray, GetType().Module);

            #region IL

            var IL = dynamicMethod.GetILGenerator();

            IL.Emit(OpCodes.Ldc_I4, handlerTypeArray.Length + 1);
            IL.Emit(OpCodes.Newarr, typeof(object));

            IL.Emit(OpCodes.Dup);
            IL.Emit(OpCodes.Ldc_I4, 0);
            IL.Emit(OpCodes.Ldc_I8, handlerPtr64);
            IL.Emit(OpCodes.Box, typeof(Int64));
            IL.Emit(OpCodes.Stelem, typeof(object));

            for (int i = 0; i < handlerTypeArray.Length; i++)
            {
                IL.Emit(OpCodes.Dup);
                IL.Emit(OpCodes.Ldc_I4, i + 1);
                IL.Emit(OpCodes.Ldarg, i);
                IL.Emit(OpCodes.Stelem, typeof(object));
            }

            IL.EmitCall(OpCodes.Call, methodInfo, null);
            IL.Emit(OpCodes.Ret);

            #endregion IL

            dynamicMethodDelegate = dynamicMethod.CreateDelegate(handlerType);

            this.source = source;

            eventInfo.AddEventHandler(source, dynamicMethodDelegate);
            return true;
        }

        private void EventInvoke(object[] args) => EventRaise?.Invoke(args);

        public static void Invoke(object args)
        {
            if (args is object[] objArr)
            {
                if (objArr.Length > 0)
                {
                    var ptr = (Int64)objArr[0];

                    if (GCHandle.FromIntPtr(new IntPtr(ptr)).Target is Action<object[]> handle)
                    {
                        handle(objArr.Skip(1).ToArray());
                    }
                }
            }
        }

        #region IDisposable

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed)
            {
                eventInfo.RemoveEventHandler(source, dynamicMethodDelegate);
                if (GCHandle.IsAllocated)
                    GCHandle.Free();

                dynamicMethodDelegate = null;
                eventInfo = null;
                source = null;
                GCHandle = new GCHandle();

                disposed = true;
            }
        }

        #endregion
    }
}