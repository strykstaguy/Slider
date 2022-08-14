using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaser : MonoBehaviour
{
    public Vector2 initDir;

    private RaycastHit2D hit;
    private LineRenderer lineRenderer, lineRenderer2, lineRenderer3;
    private Vector2 curDir, curDir2, curDir3;
    private Vector2 curPos, curPos2, curPos3;
    private void Awake() {
        lineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
        lineRenderer2 = transform.GetChild(1).GetComponent<LineRenderer>();
        lineRenderer3 = transform.GetChild(2).GetComponent<LineRenderer>();
    }
    void Update()
    {
        MakeLaser();
    }
    private void LateUpdate() 
    {
        MakeLaser();
    }
    private void MakeLaser()
    {
        Vector3 initPos = transform.position + new Vector3(-3.0f,1.0f,0.0f);
        curDir = initDir;
        curPos = initPos;
        DrawLaser(curDir, curPos, lineRenderer);
    }
    private void MakeLaser2(Vector2 initDir2)
    {
        
        Vector3 initPos2 = GameObject.FindWithTag("Portal2").transform.position + (Vector3)(initDir2*0.8f);
        
        curDir2 = initDir2;
        curPos2 = initPos2;

        DrawLaser(curDir2, curPos2, lineRenderer2);
    }

    private void DrawLaser(Vector2 dir, Vector2 pos, LineRenderer lr) 
    {
        lr.positionCount = 1;
        lr.SetPosition(0,pos);
        bool incomplete = true;
        
        while(incomplete) {
            hit = Physics2D.Raycast(pos, dir, 40.0f, 4096);
            if(hit){
                lr.positionCount += 1;
                lr.SetPosition(lr.positionCount-1, hit.collider.transform.position);
                if(hit.collider.tag == "Mirror1") {
                    dir = MirrorOneReflect(dir);
                    pos = hit.collider.transform.position + (Vector3)(dir * 1.1f);
                } else if(hit.collider.tag == "Mirror2") {
                    dir = MirrorTwoReflect(dir);
                    pos = hit.collider.transform.position + (Vector3)(dir * 1.1f);
                } else if(hit.collider.tag == "Portal1") {
                    MakeLaser2(dir);
                    incomplete = false;
                } else if(hit.collider.tag == "Portal2") {
                    incomplete = false;
                } else {
                    incomplete = false;
                }
            } else {
                lr.positionCount += 1;
                lr.SetPosition(lr.positionCount-1, pos + 34.0f*dir);
                incomplete = false;
            }
        }
    }
    private Vector2 MirrorOneReflect(Vector2 dir) {
        if(dir ==  Vector2.up){
            return Vector2.right;
        } else if(dir ==  Vector2.right){
            return Vector2.up;
        } else if(dir ==  Vector2.down){
            return Vector2.left;
        } else {
            return Vector2.down;
        }
    }
    private Vector2 MirrorTwoReflect(Vector2 dir){
        if(dir ==  Vector2.up){
            return Vector2.left;
        } else if(dir ==  Vector2.right){
            return Vector2.down;
        } else if(dir ==  Vector2.down){
            return Vector2.right;
        } else {
            return Vector2.up;
        }
    }
}

