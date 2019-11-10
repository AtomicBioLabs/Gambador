﻿using System.Collections.Generic;
using UnityEngine;

public class RaycastManager
{
    public List<string> WalkablesTags;
    public List<string> Obstaclestags;
    private GameObject player;
    private LineRenderer lr;
    private float playerHeight;
    private float lastYFromPos;
    private bool block;
    private RangeManager rangeManager;
    private GameObject gameObjectMousePositionParticle;
    private ParticleSystem particlesMouse;
    public RaycastManager()
    {
        WalkablesTags = new List<string>();
        Obstaclestags = new List<string>();
        WalkablesTags.Add("Ground");
        Obstaclestags.Add("Obstacle");
        player = GameObject.Find("Player").gameObject;
        lr = player.transform.Find("Laser").GetComponent<LineRenderer>();
        playerHeight = player.GetComponent<MeshRenderer>().bounds.size.y;
        GameManager.GameUpdate += MovingRaycastWithLineRenderer;

        rangeManager = GameManager.singleton.RangeManager;
        gameObjectMousePositionParticle = GameObject.Find("MousePositionParticle");
        particlesMouse = gameObjectMousePositionParticle.GetComponent<ParticleSystem>();
        particlesMouse.startColor = Color.green;

    }
    public Vector3 GetMousePosition(Vector3 initialPosition)
    {
        RaycastHit hit;
        Ray ray = GameManager.singleton.CameraManager.RaycastToMousePosition();
        
        if (Physics.Raycast(ray, out hit))
        {
            if (WalkablesTags.Contains(hit.transform.tag))//can move
            {
                Vector3 objectHit = new Vector3(hit.point.x, hit.point.y + 0.3f, hit.point.z);
                return objectHit;
            }
        }
        return initialPosition;
    }

    public void MovingRaycastWithLineRenderer()
    {
        Vector3 mousePos = GameManager.singleton.RaycastManager.GetMousePosition(player.transform.position);
        RaycastHit hit;
        Vector3 fromPosition = player.transform.position;
        Vector3 toPosition = mousePos;
        Vector3 direction = toPosition - fromPosition;
        Vector3 toPosition2D = new Vector3(toPosition.x, 0, toPosition.z);
        Vector3 fromPosition2D = new Vector3(fromPosition.x, 0, fromPosition.z);
        float distance2D = Vector3.Distance(toPosition2D, fromPosition2D);
        float distance = Vector3.Distance(toPosition, fromPosition);
        float radius = rangeManager.RangeRadius;
        

        player.transform.LookAt(new Vector3(mousePos.x, player.transform.position.y, mousePos.z));
        if (distance2D > radius) //If the distance is less than the radius, it is already within the circle.
        {
            Vector3 fromOriginToObject = toPosition - fromPosition; //~GreenPosition~ - *BlackCenter*
            fromOriginToObject *= radius / distance2D; //Multiply by radius //Divide by Distance
            mousePos = fromPosition + fromOriginToObject; //*BlackCenter* + all that Math
            Debug.DrawRay(new Vector3(mousePos.x, mousePos.y + 2.5f, mousePos.z), Vector3.down, Color.red);
            if (Physics.Raycast(new Vector3(mousePos.x, mousePos.y + 2.5f, mousePos.z), Vector3.down, out hit, 1000))
            {
                mousePos.y = hit.transform.position.y + playerHeight;
                block = false;
                lr.startColor = Color.green;
                lr.endColor = Color.green;
                particlesMouse.startColor = Color.green;
            }
            else
            {
                block = true;

            }
        }
        else
        {
            block = false;
        }
        if(mousePos!= player.transform.position)
        {
            gameObjectMousePositionParticle.transform.position = mousePos;
        }
        else
        {
            gameObjectMousePositionParticle.transform.position = Vector3.left * 15000;
        }
        

        lr.SetPosition(0, player.transform.position);
        lr.SetPosition(1, mousePos);
        toPosition = mousePos;
        distance = Vector3.Distance(toPosition, fromPosition);
        if (Physics.Raycast(fromPosition, direction, out hit, distance))
        {
            if (Obstaclestags.Contains(hit.transform.tag)) // there is obstacles in distance beetwen player and mouse pos
            {
                lr.startColor = Color.red;
                lr.endColor = Color.red;
                particlesMouse.startColor = Color.red;
            }
            else
            {
                if (!block)
                {
                    if (Input.GetMouseButtonDown(0)) // not an obstacle in trajectory, player can move
                    {
                        Vector3 objectHit = new Vector3(mousePos.x, mousePos.y, mousePos.z);
                        //GameManager.singleton.RangeManager.UpdateRangeSmoothly(Config.RangeIncrementBy);
                        GameManager.singleton.MovingPlayerManager.StartMovingPlayer(objectHit);
                    }
                }
                else
                {
                    lr.startColor = Color.red;
                    lr.endColor = Color.red;
                    particlesMouse.startColor = Color.red;
                }
            }
        }
        else
        {
            lr.startColor = Color.green;
            lr.endColor = Color.green;
            particlesMouse.startColor = Color.green;

            if (!block)
            {
                if (Input.GetMouseButtonDown(0)) // not an obstacle in trajectory, player can move
                {
                    Vector3 objectHit = new Vector3(mousePos.x, mousePos.y, mousePos.z);
                    //GameManager.singleton.RangeManager.UpdateRangeSmoothly(Config.RangeIncrementBy);
                    GameManager.singleton.MovingPlayerManager.StartMovingPlayer(objectHit);
                }
            }
            else
            {
                lr.startColor = Color.red;
                lr.endColor = Color.red;
                particlesMouse.startColor = Color.green;
            }
        }
    }
}
