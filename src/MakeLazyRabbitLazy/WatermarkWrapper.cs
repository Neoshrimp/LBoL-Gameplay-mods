using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MakeLazyRabbitLazy
{
    internal class WatermarkWrapper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ActivateWatermark() => AddWatermark.API.ActivateWatermark();
    }
}
