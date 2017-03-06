//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;

namespace Phonon
{
    public static class Context
    {
        public static GlobalContext GetContext()
        {
            GlobalContext context = new GlobalContext
            {
                logCallback         = IntPtr.Zero,
                allocateCallback    = IntPtr.Zero,
                freeCallback        = IntPtr.Zero
            };

            return context;
        }
    }
}