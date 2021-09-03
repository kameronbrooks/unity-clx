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

        public Vector3 GetVector()
        {
            return new Vector3();
        }

        public TestClass()
        {
            field1 = 111;
        }
    }

    [CLXAPIClass(typeof(TestClass))]
    class TestClassAPI : CSharpAPI
    {
        [APIMethod]
        public unsafe void field1_get(byte** sp, ObjectStack os)
        {
            *((int*)(*sp -= 4)) = ((TestClass)os.Pop()).field1;
        }

        [APIMethod]
        public unsafe void field1_set(byte** sp, ObjectStack os)
        {
            ((TestClass)os.Pop()).field1 = *((int*)(*sp));
            (*sp) += 4;
        }

        [APIMethod]
        public unsafe void GetVector(byte** sp, ObjectStack os)
        {
            *((Vector3*)(sp -= sizeof(Vector3))) = new Vector3();
        }

        [APIMethod]
        public unsafe void method1(byte** sp, ObjectStack os)
        {
            *((int*)(*sp -= 4)) = ((TestClass)os.Pop()).method1();
        }
    }


    [Multiline] 
    public string script;
    public string output;
    // Start is called before the first frame update
    void Start()
    {
        //RunTest2();
        
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

    public void RunTest2()
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
        Debug.Log($"CLX Time: {elapsed.TotalMilliseconds} ms");
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
