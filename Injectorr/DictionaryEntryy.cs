using System;

namespace Injectorr
{
    public class DictionaryEntryy<T, V, U> where V : T
    {
        public Type Typee { get; }
        public Object Objectt { get; }
        public object Funcc { get; }

        public DictionaryEntryy(Type typee, Type objectt, object funcc)
        {
            Typee = typee;
            Objectt = objectt;
            Funcc = funcc;
        }
    }
}