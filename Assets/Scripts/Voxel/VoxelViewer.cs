﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelViewer : MonoBehaviour {

    public void Clear()
    {
        Tool.Clear(transform);
    }

    public void SyncPos(Vector3 pos)
    {
        transform.position = pos;
    }
}
