using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLX;
using System.Runtime.CompilerServices;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
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
        Program prog = compiler.Compile(script, null);
        //Debug.Log(prog.ToString());
        Thread thread = new Thread(512);

        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        thread.Execute(prog, null);
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
