using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLX;

public class Test : MonoBehaviour
{
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
        Program prog = compiler.Compile("1+1*2;", null);
        Debug.Log(prog.ToString());
        Thread thread = new Thread(512);
        //thread.Execute(prog, null);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
