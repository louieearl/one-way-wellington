﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InputController : MonoBehaviour
{
    public static InputController Instance;

    public GameObject circleCursorPrefab;

    public GameObject backgroundGO;

    // Build measure / price interface
    public GameObject buildMeasurePrefabX;
    public GameObject buildMeasurePrefabY;
    public GameObject buildPricePrefab;

    private GameObject buildMeasureInstanceX;
    private GameObject buildMeasureInstanceY;
    private GameObject buildPriceInstance;

    public int currentBuildPrice;


    // Mouse positions 
    private Vector3 lastFramePosition;
    private Vector3 currFramePosition;
    private Vector3 dragStartPosition;

    // Build modes 
    private bool isMode_Plopable;
    private bool isMode_Dragable;

    private bool isMode_Hull;
    private bool isMode_HullNoWalls;
    private bool isMode_RemoveHull;
    private bool isMode_Wall;
    private bool isMode_RemoveWall;
    private bool isMode_Staff;
    private bool isMode_Furniture;
    private bool isMode_FurnitureMulti;
    private bool isMode_RemoveFurniture;
    private bool isMode_Rooms;




    private FurnitureType furnitureTypeInUse;
    private String roomTypeInUse;

    private GameObject staff;

    // Background scale
    private float orthoOrg;
    private float orthoCurr;
    private Vector3 scaleOrg;
    private Vector3 posOrg;


    // Camera 
    public float desiredCameraZoom;

    public bool cameraZoomEnabled;
    public float cameraSizeMin;
    public float cameraSizeMax;
    public float cameraPosZ;

    public float cameraBoundMin;
    public float cameraBoundMax;

    // Tile Interface
    public static GameObject tileInterfaceGO;
    public GameObject tileInterfacePrefab;
    





    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) Instance = this;

        // Background scale
        orthoOrg = Camera.main.orthographicSize;
        orthoCurr = orthoOrg;
        scaleOrg = backgroundGO.transform.localScale;
        posOrg = Camera.main.WorldToViewportPoint(backgroundGO.transform.position);

        cameraZoomEnabled = true;
        cameraSizeMin = 3f;
        cameraSizeMax = 50f;
        cameraPosZ = -10;
        cameraBoundMin = 0;
        cameraBoundMax = 100;

        desiredCameraZoom = Camera.main.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        // Camera zoom 
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        if (Input.GetButtonDown("Cancel"))
        {
            // Clear build modes 
            SetMode_None();

            // Clear pop-up interfaces 
            ClearUI();
        }

        if (Input.GetMouseButtonDown(0))
        {
            ClearUI();
        }

        UpdateDragging();

        UpdateCameraMovement();

        UpdateTileInterface();

        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;


    }

    void UpdateTileInterface()
    {
        // Abort if in any building mode
        if (isMode_Plopable || isMode_Dragable)
        {
            return;
        }

        if (Input.GetMouseButtonUp(0)) // LMB
        {
            // Check if player clicked on something other than a tile 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.tag != "NavigationPlane")
                {
                    return;
                }
            }

            // Get tile clicked on 
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TileOWW tileOWW = WorldController.Instance.GetWorld().GetTileAt((int)mousePos.x, (int)mousePos.y);

            // Null checks
            if (tileOWW == null)
            {
                return;
            }
            if (tileOWW.GetTileType() == "Empty" && tileOWW.currentJobType == null && tileOWW.looseItem == null)
            {
                return;
            }

            // Create interface 
            tileInterfaceGO = Instantiate(tileInterfacePrefab);
            tileInterfaceGO.GetComponent<TileInterface>().tile = tileOWW;
            tileInterfaceGO.GetComponent<TileInterface>().tileType.text = tileOWW.GetTileType();
        }
    }


    void UpdateCameraMovement()
    {
        // Handle screen panning
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {  // Right or Middle Mouse Button

            Vector3 diff = lastFramePosition - currFramePosition;

            Camera.main.transform.Translate(diff);


            // Camera bounds 
            if (Camera.main.transform.position.x < cameraBoundMin) Camera.main.transform.position = new Vector3(cameraBoundMin, Camera.main.transform.position.y, cameraPosZ);
            else if (Camera.main.transform.position.x > cameraBoundMax) Camera.main.transform.position = new Vector3(cameraBoundMax, Camera.main.transform.position.y, cameraPosZ);

            // Not contained in the same block because x and y camera bounds could be breached at the same time. 
            if (Camera.main.transform.position.y < cameraBoundMin) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, cameraBoundMin, cameraPosZ);
            else if (Camera.main.transform.position.y > cameraBoundMax) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, cameraBoundMax, cameraPosZ);
        }

        float currentCameraZoom = Camera.main.orthographicSize;

        if (cameraZoomEnabled)
        {
            // If mouse is NOT over UI element 
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                desiredCameraZoom -= desiredCameraZoom * Input.GetAxis("Mouse ScrollWheel");
            }
            
        }
        desiredCameraZoom = Mathf.Clamp(desiredCameraZoom, cameraSizeMin, cameraSizeMax);
        Camera.main.orthographicSize = Mathf.Lerp(desiredCameraZoom, currentCameraZoom, 0.5f);

        // Always update background scale, camera orthographic size may be controlled elsewhere 
        UpdateBackgroundScale();

        // Check objectives
        ObjectiveController.Instance.CheckObjectives();
    }

    public void SetMode_None()
    {
        isMode_Plopable = false;
        isMode_Dragable = false;
        isMode_Hull = false;
        isMode_HullNoWalls = false;
        isMode_RemoveHull = false;
        isMode_Wall = false;
        isMode_RemoveWall = false;
        isMode_Staff = false;
        staff = null;
        isMode_Furniture = false;
        isMode_FurnitureMulti = false;
        furnitureTypeInUse = null;
        isMode_RemoveFurniture = false;
        isMode_Rooms = false;
        roomTypeInUse = null;

        BuildModeController.Instance.SetGridVisible(false);
        BuildModeController.Instance.roomsTilemap.SetActive(false);

        BuildModeController.Instance.ClearPreviews();
    }

    public void ToggleMode_Hull()
    {
        bool temp = isMode_Hull;
        SetMode_None();
        isMode_Hull = !temp;
        isMode_Dragable = !temp;

        BuildModeController.Instance.SetGridVisible(true);
    }

    public void ToggleMode_HullNoWalls()
    {
        bool temp = isMode_HullNoWalls;
        SetMode_None();
        isMode_HullNoWalls = !temp;
        isMode_Dragable = !temp;

        BuildModeController.Instance.SetGridVisible(true);
    }

    public void ToggleMode_RemoveHull()
    {
        bool temp = isMode_RemoveHull;
        SetMode_None();
        isMode_RemoveHull = !temp;
        isMode_Dragable = !temp;

        BuildModeController.Instance.SetGridVisible(true);
    }

    public void ToggleMode_Wall()
    {
        bool temp = isMode_Wall;
        SetMode_None();
        isMode_Wall = !temp;
        isMode_Dragable = !temp;

        BuildModeController.Instance.SetGridVisible(true);
    }

    public void ToggleMode_RemoveWall()
    {
        bool temp = isMode_RemoveWall;
        SetMode_None();
        isMode_RemoveWall = !temp;
        isMode_Dragable = !temp;

        BuildModeController.Instance.SetGridVisible(true);
    }

    public void ToggleMode_Staff(GameObject staff)
    {
        bool temp = isMode_Staff;
        SetMode_None();
        isMode_Staff = !temp;
        isMode_Plopable = !temp;

        this.staff = staff;
        BuildModeController.Instance.SetGridVisible(true);
    }

    public void ToggleMode_Furniture(FurnitureType furnitureType)
    {
        if (furnitureType.multiSize)
        {
            ToggleMode_FurnitureMulti(furnitureType);
        }
        else
        {
            bool temp = isMode_Furniture;
            SetMode_None();
            isMode_Furniture = !temp;
            furnitureTypeInUse = furnitureType;
            isMode_Plopable = !temp;

            BuildModeController.Instance.SetGridVisible(true);
        }
    }

    private void ToggleMode_FurnitureMulti(FurnitureType furnitureType)
    {
        bool temp = isMode_FurnitureMulti;
        SetMode_None();
        isMode_FurnitureMulti = !temp;
        furnitureTypeInUse = furnitureType;
        isMode_Dragable = !temp;

        BuildModeController.Instance.SetGridVisible(true);

    }

    public void ToggleMode_RemoveFurniture()
    {
        bool temp = isMode_RemoveFurniture;
        SetMode_None();
        isMode_RemoveFurniture = !temp;
        isMode_Dragable = !temp;

        BuildModeController.Instance.SetGridVisible(true);

    }

    public void ToggleMode_Rooms(string roomType)
    {
        bool temp = isMode_Rooms;
        SetMode_None();
        isMode_Rooms = !temp;
        roomTypeInUse = roomType;
        isMode_Dragable = !temp;

        BuildModeController.Instance.roomsTilemap.SetActive(true);
        BuildModeController.Instance.SetGridVisible(true);
    }

    void ClearUI()
    {
        if (Passenger.passengerUIInstance != null) Destroy(Passenger.passengerUIInstance);
        if (Staff.staffUIInstance != null) Destroy(Staff.staffUIInstance);
        if (Planet.planetUI != null)
        {
            if (!Planet.planetUI.GetComponent<PlanetInterface>().isMouseOver)
            {
                Destroy(Planet.planetUI);
            }
        }
        if (tileInterfaceGO != null) Destroy(tileInterfaceGO);
    }

    void UpdateDragging()
    {
        // If we're over a UI element, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Start Drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = currFramePosition;
        }

        int start_x = Mathf.FloorToInt(dragStartPosition.x);
        int end_x = Mathf.FloorToInt(currFramePosition.x);
        int start_y = Mathf.FloorToInt(dragStartPosition.y);
        int end_y = Mathf.FloorToInt(currFramePosition.y);

        // Allows us to preview a single tile (where the mouse is) of a drag-able input mode 
        if (!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0))
        {
            start_x = end_x;
            start_y = end_y;
        }

        // We may be dragging in the "wrong" direction, so flip things if needed.
        if (end_x < start_x)
        {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }
        if (end_y < start_y)
        {
            int tmp = end_y;
            end_y = start_y;
            start_y = tmp;
        }



        // Previews for drag-able modes 

        if (isMode_Hull) BuildModeController.Instance.PreviewHull(start_x, end_x, start_y, end_y, true);
        else if (isMode_HullNoWalls) BuildModeController.Instance.PreviewHull(start_x, end_x, start_y, end_y, false);
        else if (isMode_RemoveHull) BuildModeController.Instance.PreviewRemoveHull(start_x, end_x, start_y, end_y);
        else if (isMode_Wall) BuildModeController.Instance.PreviewWall(start_x, end_x, start_y, end_y);
        else if (isMode_RemoveWall) BuildModeController.Instance.PreviewRemoveWall(start_x, end_x, start_y, end_y);
        else if (isMode_FurnitureMulti) BuildModeController.Instance.PreviewFurniture(furnitureTypeInUse, start_x, end_x, start_y, end_y);
        else if (isMode_RemoveFurniture) BuildModeController.Instance.PreviewRemoveFurniture(start_x, end_x, start_y, end_y);
        else if (isMode_Rooms) BuildModeController.Instance.PreviewRoom(roomTypeInUse, start_x, end_x, start_y, end_y);

        // Previews for plop-able modes 

        if (isMode_Furniture) BuildModeController.Instance.PreviewFurniture(furnitureTypeInUse, (int)currFramePosition.x, (int)currFramePosition.y);
        else if (isMode_Staff) BuildModeController.Instance.PreviewStaff("builder", (int)currFramePosition.x, (int)currFramePosition.y);

        if (isMode_Dragable)
        {
            // Size indicator UI (X Axis)
            if (start_x != end_x)
            {
                if (buildMeasureInstanceX == null) buildMeasureInstanceX = Instantiate(buildMeasurePrefabX);
                float xPos = ((start_x + end_x) / 2);
                xPos += ((end_x - start_x) % 2 == 0) ? 0.5f : 1f;

                buildMeasureInstanceX.transform.position = new Vector3(xPos, start_y);
                buildMeasureInstanceX.transform.localScale = Vector3.one / 60;
                buildMeasureInstanceX.GetComponentInChildren<Image>().GetComponent<RectTransform>().sizeDelta = new Vector2((end_x - start_x + 1.13f) * 60, 100);
                buildMeasureInstanceX.GetComponentInChildren<TextMeshProUGUI>().text = (end_x - start_x + 1).ToString() + "m";
            }
            else
            {
                Destroy(buildMeasureInstanceX);
            }
            // Size indicator UI (Y Axis)
            if (start_y != end_y)
            {
                if (buildMeasureInstanceY == null) buildMeasureInstanceY = Instantiate(buildMeasurePrefabY);
                float yPos = ((start_y + end_y) / 2);
                yPos += ((end_y - start_y) % 2 == 0) ? 0.5f : 1f;

                buildMeasureInstanceY.transform.position = new Vector3(start_x, yPos);
                buildMeasureInstanceY.transform.localScale = Vector3.one / 60;
                buildMeasureInstanceY.GetComponentInChildren<Image>().GetComponent<RectTransform>().sizeDelta = new Vector2((end_y - start_y + 1.13f) * 60, 100);
                buildMeasureInstanceY.GetComponentInChildren<TextMeshProUGUI>().text = (end_y - start_y + 1).ToString() + "m";

                Debug.Log(buildMeasureInstanceY.transform.rotation);
            }
            else
            {
                Destroy(buildMeasureInstanceY);
            }

            // Price indicator UI 
            if (buildPriceInstance == null) buildPriceInstance = Instantiate(buildPricePrefab);
            float pricePosX = ((start_x + end_x) / 2);
            pricePosX += ((end_x - start_x) % 2 == 0) ? 0.5f : 1f;
            float pricePosY = ((start_y + end_y) / 2);
            pricePosY += ((end_y - start_y) % 2 == 0) ? 0.5f : 1f;
            buildPriceInstance.transform.position = new Vector3(pricePosX, pricePosY);
            buildPriceInstance.transform.localScale = Vector3.one / 60;
            buildPriceInstance.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("{0:C}", currentBuildPrice);
        }

        // Cancel drag
        if (Input.GetButtonDown("Cancel"))
        {
            SetMode_None();
            BuildModeController.Instance.ClearPreviews();
            Destroy(buildMeasureInstanceX);
            Destroy(buildMeasureInstanceY);
            return;
        }

        // End Drag
        if (Input.GetMouseButtonUp(0))
        {

            if (isMode_Hull)
            {
                Tuple<List<TileOWW>, List<TileOWW>, List<TileOWW>> hull_tuple;
                hull_tuple = BuildModeController.Instance.PreviewHull(start_x, end_x, start_y, end_y, true);
                if (hull_tuple != null)
                {
                    BuildModeController.Instance.PlanHull(hull_tuple.Item1, hull_tuple.Item2, hull_tuple.Item3);
                }
            }
            if (isMode_HullNoWalls)
            {
                Tuple<List<TileOWW>, List<TileOWW>, List<TileOWW>> hull_tuple;
                hull_tuple = BuildModeController.Instance.PreviewHull(start_x, end_x, start_y, end_y, false);
                if (hull_tuple != null)
                {
                    BuildModeController.Instance.PlanHull(hull_tuple.Item1, hull_tuple.Item2, hull_tuple.Item3);
                }
            }
            else if (isMode_RemoveHull)
            {
                List<TileOWW> hull_tiles;
                hull_tiles = BuildModeController.Instance.PreviewRemoveHull(start_x, end_x, start_y, end_y);
                BuildModeController.Instance.PlanRemoveHull(hull_tiles);

            }
            else if (isMode_Wall)
            {
                List<TileOWW> wall_tiles = BuildModeController.Instance.PreviewWall(start_x, end_x, start_y, end_y);
                BuildModeController.Instance.PlanFurniture(wall_tiles, BuildModeController.Instance.furnitureTypes["Wall"]);
            }
            else if (isMode_RemoveWall)
            {
                List<TileOWW> wall_tiles = BuildModeController.Instance.PreviewRemoveWall(start_x, end_x, start_y, end_y);
                BuildModeController.Instance.PlanRemoveFurniture(wall_tiles);
            }
            else if (isMode_Staff)
            {
                // Generate an unused staff name 
                int staffNumber = 0;
                bool staffNumberCollides = true;
                while (staffNumberCollides == true)
                {
                    staffNumber = UnityEngine.Random.Range(1, 999);
                    if (WorldController.Instance.staff.Count == 0)
                    {
                        staffNumberCollides = false;
                    }
                    foreach (GameObject staffGO in WorldController.Instance.staff)
                    {
                        if (staffGO.name.Contains(staffNumber.ToString()))
                        {
                            staffNumberCollides = true;
                            break;
                        }
                        staffNumberCollides = false;
                    }
                }

                string staffName = staff.tag + staffNumber.ToString();

                BuildModeController.Instance.PlaceStaff((int)currFramePosition.x, (int)currFramePosition.y, staff, staffName);
            }
            else if (isMode_Furniture)
            {
                TileOWW furniture_tile = BuildModeController.Instance.PreviewFurniture(furnitureTypeInUse, (int)currFramePosition.x, (int)currFramePosition.y);
                BuildModeController.Instance.PlanFurniture(furniture_tile, furnitureTypeInUse);
            }
            else if (isMode_FurnitureMulti)
            {
                List<TileOWW> furnitureTiles = BuildModeController.Instance.PreviewFurniture(furnitureTypeInUse, start_x, end_x, start_y, end_y);
                BuildModeController.Instance.PlanFurniture(furnitureTiles, furnitureTypeInUse);
            }
            else if (isMode_RemoveFurniture)
            {
                List<TileOWW> wall_tiles = BuildModeController.Instance.PreviewRemoveFurniture(start_x, end_x, start_y, end_y);
                BuildModeController.Instance.PlanRemoveFurniture(wall_tiles);
            }
            else if (isMode_Rooms)
            {
                List<TileOWW> roomTiles = BuildModeController.Instance.PreviewRoom(roomTypeInUse, start_x, end_x, start_y, end_y);
                BuildModeController.Instance.PlaceRoom(roomTiles, roomTypeInUse);
            }

            BuildModeController.Instance.ClearPreviews();
        }
    }

    public void UpdateBackgroundScale()
    {
        var osize = Camera.main.orthographicSize;
        if (orthoCurr != osize)
        {
            backgroundGO.transform.localScale = scaleOrg * osize / orthoOrg;
            orthoCurr = osize;
            backgroundGO.transform.position = Camera.main.ViewportToWorldPoint(posOrg);
        }
    }

    public IEnumerator MoveCameraTo(float cameraPosX, float cameraPosY)
    {
        Vector3 newCameraPos = new Vector3(cameraPosX, cameraPosY, -10);
    
        while (Vector3.Distance(Camera.main.transform.position, newCameraPos) > 0.1f)
        {
            Camera.main.transform.position = Vector3.Lerp(newCameraPos, Camera.main.transform.position, 0.5f);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        
    }


    
}
