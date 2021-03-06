﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditOperation { Use,NotUse, Reverse }

public class VoxelBuilder : MonoBehaviour {

    public static float xUnit = 1.0f;
    public static float yUnit = 1.5f;
    public static float heightUnit = 0.5f;

    [SerializeField]
    VoxelViewer voxelViewer;

    [SerializeField]
    EditOperation operation= EditOperation.Use;
    public EditOperation GetOperation() { return operation; }

    public delegate void FuncPtr(Voxel node);
    public void DoOperation(Voxel node, FuncPtr func) {
        func(node);
    }

    [SerializeField]
    int addCountX = 1;

    [SerializeField]
    int addCountY = 1;

    public int GetAddCountX() { return addCountX; }
    public int GetAddCountY() { return addCountY; }

    [SerializeField]
    GameObject voxelVixibleOdd;
    [SerializeField]
    GameObject voxelVisibleEven;
    [SerializeField]
    Voxel voxel;

    [SerializeField]
    [HideInInspector]
    private Voxel[] map3D;

    public int CountY() { return 2 * Y - 1; }
    public int CountX() { return 2 * X - 1; }

    public void SyncPos()
    {
        voxelViewer.SyncPos(transform.position);
    }

    public void GenerateMap()
    {
        nowFloorIndex = 0;
        Tool.Clear(transform);
        voxelViewer.Clear();

        map3D = new Voxel[Floor* CountY()* CountX()];
        var original = transform.position;
        var offsetX = 0.5f * Vector3.right * VoxelBuilder.xUnit;
        var offsetY = 0.5f * Vector3.forward * VoxelBuilder.yUnit;
        var offsetFloor =  Vector3.up * VoxelBuilder.heightUnit;
        var offsetXY = offsetX + offsetY;
        for (var f = 0; f < Floor; ++f) {
            bool isOdd = f % 2 == 0;
            for (var y = 0; y < CountY(); ++y){
                for (var x = 0; x < CountX(); ++x){
                    var node =Instantiate<Voxel>(voxel);
                    node.transform.position = original + offsetXY + offsetFloor * f + offsetY * y + offsetX * x;
                    node.transform.parent = transform;
                    node.Init(f, y, x, isOdd );
                    var index = ReMap(f, y, x);
                    map3D[index] = node;
                }
            }
        }
    }


    int ReMap(int floorIndex, int y, int x) {
        return floorIndex * CountY() * CountX() + y *CountX() + x;
    }

    public bool IsSetValue(int floorIndex, int y,int x)
    {
        if (map3D == null)
            return false;

        var index = ReMap(floorIndex, y, x);
        return map3D[index].IsUse();
    }

    [SerializeField]
    int X=10;
    [SerializeField]
    int Y=10;//Z方向定義成Y
    [SerializeField]
    int Floor = 10;//Y方向定義成Height

    [SerializeField]
    int nowFloorIndex = 0;
    public int GetNowFloorIndex() { return nowFloorIndex; }
    public void SetNowFloorIndex(int offset) {
        var newIndex = nowFloorIndex + offset;
        if (IsValidatedFloorIndex(newIndex))
            nowFloorIndex = newIndex;
    }
    public bool IsValidatedFloorIndex(int index) { return index >= 0 && index < Floor; }
    public bool IsValidatedX(int x) { return x >= 0 && x < CountX(); }
    public bool IsValidatedY(int y) { return y >= 0 && y < CountY(); }

    public Voxel GetVoxel(int floor, int y, int x) {
        if (IsValidatedY(y) && IsValidatedX(x))
        {
            var index = ReMap(floor, y, x);
            return map3D[index];
        }
        return null;
    }

    public int GetX() { return X; }
    public int GetY() { return Y; }
    public int GetFloor() { return Floor; }
    public Vector3 GetNowFlowerHeight() { return Vector3.up * VoxelBuilder.heightUnit * nowFloorIndex; }

    Vector3 clickPointOnRay;
    public Vector3 GetClickPointOnRay() { return clickPointOnRay; }
    public void SetClickPointOnRay(Vector3 p) { clickPointOnRay = p; }

    Vector3 clickNormalDir;
    public Vector3 GetClickNormalDir() { return clickNormalDir; }
    public void SetClickNormalDir(Vector3 normalDir) { clickNormalDir = normalDir; }

    Vector3 hitPoint;
    public Vector3 GetHitPoint() { return hitPoint; }

    public bool IsCanUse(Voxel node) {
        var x = node.x;
        var y = node.y;
        var f = node.floor;

        //8個角都沒在使用才行
        var node8 = new Voxel[] { GetVoxel(f, y, x-1) ,GetVoxel(f, y, x+1) ,
                                    GetVoxel(f, y-1, x) ,GetVoxel(f, y+1, x) ,
                                    GetVoxel(f, y-1, x-1) ,
                                    GetVoxel(f, y+1, x+1) ,
                                    GetVoxel(f, y-1, x+1) ,
                                    GetVoxel(f, y+1, x-1) } ;
        for(var i = 0; i < node8.Length; ++i)
        {
            var nowNode = node8[i];
            if (nowNode == null)
                continue;

            if (nowNode.IsUse())
                return false;
        }
        return true;
    }

    public bool DoClick(Vector3 from ,Vector3 dir,out int floor,out int y,out int x,bool onlyUseVoxel = false)
    {
        floor = -1;
        x = -1;
        y = -1;

        bool hit = GeometryTool.RayHitPlane(from, dir, Vector3.up, transform.position+ GetNowFlowerHeight(), out hitPoint);
        if (!hit)
            return false;

        var voxel= GetHitVoxel(hitPoint);
        if (voxel == null)
            return false;

        if (onlyUseVoxel)
            if (!voxel.IsUse())
                return false;

        bool hitSphere = voxel.IsHit(hitPoint);
        if (!hitSphere)
            return false;

        x = voxel.x;
        y = voxel.y;
        floor = voxel.floor;

        return true;
    }

    Voxel GetHitVoxel(Vector3 hitPoint)
    {
        var offsetX = 0.25f * Vector3.right * VoxelBuilder.xUnit;
        var offsetY = 0.25f * Vector3.forward * VoxelBuilder.yUnit;
        var refPoint = transform.position + offsetX + offsetY;

        var diff = (hitPoint - refPoint);
        var halfXUnit = 0.5 * VoxelBuilder.xUnit;
        var halfYUnit = 0.5 * VoxelBuilder.yUnit;
        var x = (int)((diff.x - (diff.x % halfXUnit)) / halfXUnit);
        var y = (int)((diff.z - (diff.z % halfYUnit)) / halfYUnit);

        return GetVoxel(nowFloorIndex, y, x);
    }

    public void ReverseNode(Voxel node)
    {
        bool canUse = IsCanUse(node);
        if (!canUse)
            return;

        if (node.IsUse())
        {
            node.SetIsUse(false);
            RemoveVisible(node);
        }
        else
        {
            node.SetIsUse(true);
            AddVisible(node);
        }
            
    }

    public void UseNode(Voxel node)
    {
        bool canUse = IsCanUse(node);
        if (!canUse)
            return;

        node.SetIsUse(true);
        AddVisible(node);
    }

    public void NotUseNode(Voxel node)
    {
        bool canUse = IsCanUse(node);
        if (!canUse)
            return;

        node.SetIsUse(false);
        RemoveVisible(node);
    }

    void AddVisible(Voxel node)
    {
        if (node.visible != null)
            return;

        var obj =Instantiate<GameObject>(node.IsOdd() ? voxelVixibleOdd : voxelVisibleEven,voxelViewer.transform);
        obj.transform.localPosition = node.transform.localPosition;
        obj.name = node.name;
        node.visible = obj;
    }

    void RemoveVisible(Voxel node)
    {
        DestroyImmediate(node.visible);
        node.visible = null;
    }
}
