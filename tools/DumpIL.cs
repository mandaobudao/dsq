using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

// Usage: DumpIL.exe <dll> Type::Method [Type::Method ...]
// Dumps IL instructions of the given methods.
internal static class DumpIL
{
    private static int Main(string[] args)
    {
        string dll = args[0];
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(dll));
        var rp = new ReaderParameters { AssemblyResolver = resolver, ReadingMode = ReadingMode.Immediate };
        using (var asm = AssemblyDefinition.ReadAssembly(dll, rp))
        {
            foreach (string spec in args.Skip(1))
            {
                int sep = spec.IndexOf("::", StringComparison.Ordinal);
                if (sep < 0) { Console.WriteLine("BAD SPEC: " + spec); continue; }
                string tname = spec.Substring(0, sep);
                string mname = spec.Substring(sep + 2);

                TypeDefinition t = asm.MainModule.Types.FirstOrDefault(x => x.Name == tname)
                    ?? asm.MainModule.Types.FirstOrDefault(x => x.FullName == tname);
                if (t == null) { Console.WriteLine("===== TYPE NOT FOUND: " + tname); continue; }

                foreach (MethodDefinition m in t.Methods.Where(m => m.Name == mname))
                {
                    Console.WriteLine("===== " + t.FullName + "::" + m.Name + m.FullName.Substring(m.FullName.IndexOf('(')));
                    if (!m.HasBody) { Console.WriteLine("  (no body)"); continue; }
                    foreach (Instruction ins in m.Body.Instructions)
                    {
                        string op = "";
                        if (ins.Operand is MethodReference mr) op = mr.DeclaringType.Name + "::" + mr.Name;
                        else if (ins.Operand is FieldReference fr) op = fr.DeclaringType.Name + "::" + fr.Name;
                        else if (ins.Operand != null) op = ins.Operand.ToString();
                        Console.WriteLine($"  IL_{ins.Offset:X4}  {ins.OpCode,-14} {op}");
                    }
                }
            }
        }
        return 0;
    }
}
