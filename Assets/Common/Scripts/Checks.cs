using System;

namespace Common.Scripts
{
    public abstract class Checks
    {
        public static void CheckNotNull(object obj, string error)
        {
            if (obj is null)
            {
                throw new Exception(error);
            }
        }
    }
}
