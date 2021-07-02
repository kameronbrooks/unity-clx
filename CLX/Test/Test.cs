using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLX;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    [Multiline] 
    public string script;
    public string output;
    // Start is called before the first frame update
    void Start()
    {

        
    }


    public void RunTest1()
    {
        Runtime runtime = new Runtime();

        InstructionBuffer buffer = new InstructionBuffer();

        Compiler compiler = new Compiler();
        Program prog = compiler.Compile(script, null);
        Debug.Log(prog.ToString());
        Thread thread = new Thread(512);
        thread.Execute(prog, null);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
