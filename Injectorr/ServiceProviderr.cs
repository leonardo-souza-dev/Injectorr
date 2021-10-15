using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Injectorr
{
    public class ServiceProviderr
    {
        private static readonly List<DictionaryEntryy<Type, Type, Type>> _container = new();
        
        private static object ObterInstancia(Type interfacee)
        {
            var classeConcreta = ResolveTypee(interfacee);

            if (classeConcreta.GetConstructors().Any())
            {
                var construtor = classeConcreta.GetConstructors()[0];
                var parametros = construtor.GetParameters();

                if (parametros.Any())
                {
                    var parametrosInstanciados = new List<object>();
                    
                    foreach (var parametroDoCtorDoTipo in parametros)
                    {
                        var parametroInstanciado = ObterParametroInstanciado(interfacee, parametroDoCtorDoTipo);
                        parametrosInstanciados.Add(parametroInstanciado);
                    }
                    var instancia = construtor.Invoke(parametrosInstanciados.ToArray());
                    return instancia;
                }
                else
                {
                    var instancia = construtor.Invoke(null);
                    return instancia;
                }
            }
            
            return null;
        }

        private static object ObterParametroInstanciado(Type tipoObterInstancia, ParameterInfo parametroDoCtorDoTipo)
        {
            object parametroInstanciado;
            Type tipoDoParametro = parametroDoCtorDoTipo.ParameterType;

            var itemm = _container.FirstOrDefault(x => x.Typee.FullName == tipoObterInstancia.FullName);

            if (NaoEhTipoInterno(tipoDoParametro))
            {
                parametroInstanciado = ObterInstanciaDoParametroQuandoNaoEhTipoInterno(parametroDoCtorDoTipo, itemm);
            }
            else
            {
                parametroInstanciado = ObterInstanciaDoParametroQuandoEhTipoInterno(itemm);
            }

            return parametroInstanciado;
        }

        private static object ObterInstanciaDoParametroQuandoEhTipoInterno(DictionaryEntryy<Type, Type, Type>? itemm)
        {
            object parametroInstanciado = null;
            if (itemm != null)
            {
                var obj = (Func<Type, object>) itemm.Funcc;
                var invocationList = obj.GetInvocationList();
                foreach (var invocationItem in invocationList)
                {
                    var tgt = invocationItem.Target;
                    var fieldName = tgt?.GetType().GetFields()[0].Name;
                    parametroInstanciado = tgt?.GetType().GetField(fieldName)?.GetValue(tgt);
                }
            }

            return parametroInstanciado;
        }
        
        private static object ObterInstanciaDoParametroQuandoNaoEhTipoInterno(
            ParameterInfo parametroDoCtorDoTipo, 
            DictionaryEntryy<Type, Type, Type>? item)
        {
            object parametroInstanciado = null;
            var customCtor = (Func<Type, object>)item.Funcc;
            
            if (item != null && customCtor != null)
            {
                parametroInstanciado = ObterParametroInstanciadoQuandoNaoEhTipoInterno_CustomCtor(
                    parametroDoCtorDoTipo, 
                    customCtor.GetInvocationList(), 
                    parametroInstanciado);
            }
            else
            {
                parametroInstanciado = ObterInstancia(parametroDoCtorDoTipo.ParameterType);
            }

            return parametroInstanciado;
        }

        private static object ObterParametroInstanciadoQuandoNaoEhTipoInterno_CustomCtor(
            ParameterInfo parametroDoCtorDoTipo, 
            Delegate[] invocationList, object parametroInstanciado)
        {
            foreach (var invocationItem in invocationList)
            {
                var tgt = invocationItem.Target;
                var campos = tgt?.GetType().GetFields();

                if (campos == null)
                    throw new ApplicationException("Campos nulos");

                var nomeCampo = "";
                foreach (var campo in campos)
                {
                    if (campo.Name == parametroDoCtorDoTipo.Name)
                    {
                        nomeCampo = campo.Name;
                    }
                }

                parametroInstanciado = tgt?.GetType().GetField(nomeCampo)?.GetValue(tgt);
            }

            return parametroInstanciado;
        }

        public static void Map<T, V>() where V : T
        {
            _container.Add(new DictionaryEntryy<Type, Type, Type>(typeof(T), typeof(V), null));
        }

        public static void Map<T, V>(Func<object, V> func)
        {
            _container.Add(new DictionaryEntryy<Type, Type, Type>(typeof(T), typeof(V), func));
        }

        public static void Clear()
        {
            _container.Clear();
        }
        
        public static T ObterInstancia<T>()
        {
            var type = typeof(T);
            var instancia = (T)ObterInstancia(type);
            
            return instancia;
        }

        private static List<Type> tiposInternos = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(nint),
            typeof(nuint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(object),
            typeof(string),
            typeof(Boolean[]), 
            typeof(Byte[]),
            typeof(SByte[]),
            typeof(Char[]),
            typeof(Decimal[]), 
            typeof(Double[]),
            typeof(Single[]),
            typeof(Int32[]),
            typeof(UInt32[]),
            typeof(nint[]),
            typeof(nuint[]),
            typeof(Int64[]),
            typeof(UInt64[]),
            typeof(Int16[]),
            typeof(UInt16[]),
            typeof(Object[]),
            typeof(String[])
        };

        private static bool NaoEhTipoInterno(Type typee)
        {
            return !tiposInternos.Contains(typee);
        }
        private static Type ResolveTypee(Type type)
        {
            foreach (var item in _container)
            {
                if (item.Typee.FullName == type.FullName)
                {
                    return (Type) item.Objectt; 
                }
            }

            return type;
        }
    }
}