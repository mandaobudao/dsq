using System;
using System.Linq;
using Mono.Cecil;

internal static class CecilQuery
{
    private static int Main(string[] args)
    {
        string dll = args[0];
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(dll));
        var rp = new ReaderParameters { AssemblyResolver = resolver, ReadingMode = ReadingMode.Deferred };
        using (var asm = AssemblyDefinition.ReadAssembly(dll, rp))
        {
            foreach (string name in args.Skip(1))
            {
                if (name == "*")
                {
                    foreach (var td in asm.MainModule.Types)
                        Console.WriteLine("TYPE " + td.FullName);
                    continue;
                }
                TypeDefinition t = asm.MainModule.Types.FirstOrDefault(x => x.FullName == name)
                    ?? asm.MainModule.Types.FirstOrDefault(x => x.Name == name);
                if (t == null)
                {
                    Console.WriteLine("===== " + name + " NOT FOUND");
                    continue;
                }
                Console.WriteLine("===== " + t.FullName + (t.IsEnum ? " ENUM" : ""));
                foreach (var f in t.Fields)
                {
                    if (f.IsSpecialName) continue;
                    Console.WriteLine("  F " + (f.IsPublic ? "pub" : f.IsFamily ? "fam" : "pri")
                        + (f.IsStatic ? " static" : "") + " " + f.FieldType.Name + " " + f.Name);
                }
                foreach (var m in t.Methods)
                {
                    if (m.IsSpecialName)
                    {
                        Console.WriteLine("  P " + (m.IsPublic ? "pub" : "pri") + " " + m.ReturnType.Name + " " + m.Name);
                        continue;
                    }
                    var ps = string.Join(", ", m.Parameters.Select(p => p.ParameterType.Name + " " + p.Name));
                    Console.WriteLine("  M " + (m.IsPublic ? "pub" : m.IsFamily ? "fam" : "pri")
                        + (m.IsStatic ? " static" : "") + " " + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
                }
            }
        }
        return 0;
    }
}
