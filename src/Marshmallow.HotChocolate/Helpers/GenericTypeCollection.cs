using System;
using System.Collections.Generic;
using System.Text;

namespace Marshmallow.HotChocolate.Helpers
{
    class GenericTypeCollection
    {
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public void AddIfNotExists(string key, Type type)
        {
            if (!_types.ContainsKey(key))
            {
                _types.Add(key, type);
            }
        }

        public Type Load(string key)
        {
            return _types[key];
        }
    }
}
