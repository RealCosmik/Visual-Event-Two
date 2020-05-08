using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace VisualDelegates
{
    internal class MethodParamEquaility : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[] x, Type[] y)
        {
            if (x.Length != y.Length)
                return false;
            else
            {
                int length = x.Length;
                bool isequal = true;
                for (int i = 0; i < length; i++)
                {
                    if (x[i] != y[i])
                    {
                        isequal = false;
                        break;
                    }
                }
                return isequal;
            }
        }

        public int GetHashCode(Type[] obj)
        {
            int hash = 17;
            int length = obj?.Length ?? 0;
            for (int i = 0; i < length; i++)
            {
                unchecked
                {
                    hash = hash * 23 + obj[i].GetHashCode();
                }
            }
            return hash;
        }
    }
}
