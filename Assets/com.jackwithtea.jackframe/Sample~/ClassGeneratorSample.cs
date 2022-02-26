
namespace JackFrame.Sample {

    public class ClassGeneratorSample {

        public void Test() {

            ClassGenerator generator = new ClassGenerator(VisitLevel.Public, "MyClass");

            generator.SetNamespace("POG.Demo");

            generator.SetUsing("System", "System.Collection.Generic", "UnityEngine");

            generator.SetField(VisitLevel.Public, "float", "lifeTime");

            MethodGenerator box = new MethodGenerator(VisitLevel.Public, "int", "GetCount", "List<string> list", "int[] arr");
            box.AppendLine("return list.Count + arr.Length;");
            generator.SetMethod(box);

            PLog.Log(generator.ToString());

        }

    }
}