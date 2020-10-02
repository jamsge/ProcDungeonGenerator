using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Tree {
    public int x;
    public int z;
    public int w;
    public int h;
    public int centerX;
    public int centerZ;
    public char split;
    public int parentIndex;
    public int l;
    public int r;
    public bool isLeaf;
    public bool hasChildren;

    public Tree(int x, int z, int w, int h, char split = ' ', bool isLeaf = false) {
        this.x = x;
        this.z = z;
        this.w = w;
        this.h = h;
        this.centerX = -1;
        this.centerZ = -1;
        this.split = split;
        this.parentIndex = -1; 
        this.l = -1;
        this.r = -1;
        this.isLeaf = isLeaf;
        this.hasChildren = true;
    }
}
