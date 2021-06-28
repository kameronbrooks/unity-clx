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

        buffer.Add(OpCode.LdC_i32, 10);
        buffer.Add(OpCode.LdC_i32, 15);
        buffer.Add(OpCode.Add_i32);
        buffer.Add(OpCode.Prnt_i32);
        buffer.Add(OpCode.Term);


        Program prog = new Program();
        prog.instructions = buffer.BakeAndExport();
        prog.minStackSize = 512;
        Thread thread = new Thread(512);
        thread.Execute(prog, null);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
