using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3_API : CLX.CSharpAPI
{
    Dictionary<string, CLX.CSharpAPI.API_CALL> map;
    public Vector3_API()
    {

    }
    [CLX.APIMethod]
    public unsafe void x_get(byte** sp, CLX.ObjectStack os)
    {
        Vector3 vec = *((Vector3*)sp);
        sp += sizeof(Vector3);
        *((float*)(sp -= sizeof(float))) = vec.x;
    }
    [CLX.APIMethod]
    public unsafe void x_set(byte** sp, CLX.ObjectStack os)
    {
        sp += sizeof(float);
        (*((Vector3*)(sp - sizeof(Vector3)))).x = *((float*)sp);
    }
    [CLX.APIMethod]
    public unsafe void y_get(byte** sp, CLX.ObjectStack os)
    {
        Vector3 vec = *((Vector3*)sp);
        sp += sizeof(Vector3);
        *((float*)(sp -= sizeof(float))) = vec.y;
    }
    [CLX.APIMethod]
    public unsafe void y_set(byte** sp, CLX.ObjectStack os)
    {
        sp += sizeof(float);
        (*((Vector3*)(sp - sizeof(Vector3)))).y = *((float*)sp);
    }
    [CLX.APIMethod]
    public unsafe void z_get(byte** sp, CLX.ObjectStack os)
    {
        Vector3 vec = *((Vector3*)sp);
        sp += sizeof(Vector3);
        *((float*)(sp -= sizeof(float))) = vec.z;
    }
    [CLX.APIMethod]
    public unsafe void z_set(byte** sp, CLX.ObjectStack os)
    {
        sp += sizeof(float);
        (*((Vector3*)(sp - sizeof(Vector3)))).z = *((float*)sp);
    }

    public override unsafe void __Construct__(byte** sp, CLX.ObjectStack os)
    {
        *((Vector3*)(sp -= sizeof(Vector3))) = new Vector3();
    }

    [CLX.APIMethod]
    public unsafe void magnitude(byte** sp, CLX.ObjectStack os)
    {
        Vector3 vec = *((Vector3*)sp);
        sp += sizeof(Vector3);
        *((float*)(sp -= sizeof(float))) = vec.magnitude;
    }

}
