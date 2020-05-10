using System;
using System.IO;

namespace LzmaStreamTest
{
    public static class Utility
    {
        public static Stream OpenSiblingResourceStream(this Type t, string name)
        {
            return t.Assembly.GetManifestResourceStream(t, name);
        }
    }
}