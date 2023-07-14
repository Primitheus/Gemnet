using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Network
{

    internal class Network
    {

    }

    public static class ReflectionHelpers
    {
        public static IEnumerable<Type> GetImplementors(this Type abstractType, params Assembly[] assembliesToSearch)
        {
            var typesInAssemblies = assembliesToSearch.SelectMany(assembly => assembly.GetTypes());
            return typesInAssemblies.Where(abstractType.IsAssignableFrom);
        }
    }

}
