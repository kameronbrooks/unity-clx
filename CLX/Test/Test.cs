using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLX;
using System.Runtime.CompilerServices;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    class TestClass
    {
        public int field1;

        private int privatField;
        public int prop1
        {
            get
            {
                return privatField;
            }
            set
            {
                privatField = value;
            }
        }
        [APIMethod]
        public int method1()
        {
            return 10;
        }

        public void Method2()
        {

        }
    }

    [CLXAPIClass(typeof(TestClass))]
    class TestClassAPI : CSharpAPI
    {
        public override unsafe API_CALL GetField(string name)
        {
            if(name == "field1")
            {
                return (a, b) =>
                {
                    *((int*)(*a -= 4)) = ((TestClass)b.Pop()).field1;
                };
            }

            throw new System.Exception($"Unsupported api call {name} in api_class {this.targetType.Name}");
        }

        public override unsafe API_CALL GetProperty(string name)
        {
            if (name == "prop1")
            {
                return (a, b) =>
                {
                    *((int*)(*a -= 4)) = ((TestClass)b.Pop()).prop1;
                };
            }

            throw new System.Exception($"Unsupported api call {name} in api_class {this.targetType.Name}");
        }

        public override unsafe API_CALL Method(string name, params System.Type[] signature)
        {
            throw new System.Exception($"Unsupported api call {name} in api_class {this.targetType.Name}");
        }

        public override unsafe API_CALL SetField(string name)
        {
            throw new System.Exception($"Unsupported api call {name} in api_class {this.targetType.Name}");
        }

        public override unsafe API_CALL SetProperty(string name)
        {
            if (name == "prop1")
            {
                return (a, b) =>
                {
                    ((TestClass)b.Pop()).prop1 = *((int*)*a);
                    *a += 4;
                };
            }

            throw new System.Exception($"Unsupported api call {name} in api_class {this.targetType.Name}");
        }

        
            
    }


    [Multiline] 
    public string script;
    public string output;
    // Start is called before the first frame update
    void Start()
    {
        RunTest1();
        
    }

    


    public void RunTest1()
    {
        Runtime runtime = new Runtime();

        InstructionBuffer buffer = new InstructionBuffer();



        Compiler compiler = new Compiler();
        Program prog = compiler.Compile(script, typeof(TestClass));
        //Debug.Log(prog.ToString());
        Thread thread = new Thread(512);

        TestClass testTarget = new TestClass();

        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        thread.Execute(prog, testTarget);
        System.TimeSpan elapsed = watch.Elapsed;
        watch.Stop();
        Debug.Log($"CLX Time: {elapsed.TotalMilliseconds}ms");

        watch = System.Diagnostics.Stopwatch.StartNew();
        int i = CSharpTest();
        elapsed = watch.Elapsed;
        watch.Stop();
        Debug.Log($"Control Time: {elapsed.TotalMilliseconds}ms");
        Debug.Log(i);
    }

    //[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public int CSharpTest()
    {

        int i;
        i = 12345;
        int j;
        j = 12345;

        int h;
        h = (i + j) * (i - 2);
        return h;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
