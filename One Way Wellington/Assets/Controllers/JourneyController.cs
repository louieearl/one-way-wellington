﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JourneyController : MonoBehaviour
{

    public static JourneyController Instance;

    public Planet earth;
    public Planet lastPlanetVisit;
    public Planet nextPlanetVisit;
    public Planet currentPlanetVisit;
    public bool isAtOriginPlanet;
    public Vector2 shipCoordinates = Vector2.zero;
    public float shipSpeedMax = 0; 
    public float shipSpeedCurrent;
    public bool isJourneyEditMode; // TODO: set ONLY after Wellington arrival

    public GameObject orangeLinePrefab;
    public GameObject progressMarkerPrefab;
    public GameObject PlanetUIPrefab;

    public GameObject distanceRingPrefab;
    public GameObject distanceRingInstance;

    public List<GameObject> currentPassengers;

    public float fuelRemaining;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) Instance = this;
        progressMarkerPrefab = Instantiate(progressMarkerPrefab);
        progressMarkerPrefab.transform.position = new Vector3(0, 0, 1020);

        currentPassengers = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (nextPlanetVisit != null && !isJourneyEditMode)
        {
            if (Vector2.Distance(shipCoordinates, nextPlanetVisit.GetPlanetCoordinates()) < 0.1)
            {
                // We have arrived at a planet! 
                currentPlanetVisit = nextPlanetVisit;

                shipSpeedCurrent = 0;
                isJourneyEditMode = true;
                StartCoroutine(TransitionController.Instance.TransitionArrival(nextPlanetVisit));
                UserInterfaceController.Instance.ShowLandUI();
                currentPlanetVisit = nextPlanetVisit;
                
            }
            else
            {
                // Move towards the planet 

                shipCoordinates = Vector2.MoveTowards(shipCoordinates, nextPlanetVisit.GetPlanetCoordinates(), (shipSpeedCurrent / 5) * Time.deltaTime);
                progressMarkerPrefab.transform.position = new Vector3(shipCoordinates.x, shipCoordinates.y, 1020);
                currentPlanetVisit = null;
            }
        }
    }

    public void AddNextPlanet(Planet planet)
    {
        if (isJourneyEditMode)
        {
            
            Planet lastPlanet = earth.GetNextPlanet();
            if (fuelRemaining > Vector3.Distance(planet.transform.position, lastPlanet.transform.position))
            {
                if (earth.SetNextPlanet(planet, earth)) // If successful 
                {
                    lastPlanetVisit = planet;
                    nextPlanetVisit = lastPlanet;

                    // Switch UI buttons
                    UserInterfaceController.Instance.panel_LaunchJourney.SetActive(true);
                    UserInterfaceController.Instance.panel_GoToShip.SetActive(false);

                    // Draw line between planets 
                    GameObject line = Instantiate(orangeLinePrefab);
                    line.transform.position = Vector3.Lerp(planet.transform.position, lastPlanet.transform.position, 0.5f);
                    line.transform.localScale = new Vector3(Vector3.Distance(planet.transform.position, lastPlanet.transform.position) * 100, 500, 1);
                    line.transform.LookAt(planet.transform.position);
                    line.transform.Rotate(new Vector3(0, 90, 0));
                    line.transform.parent = TransitionController.Instance.mapGO.transform;
                    nextPlanetVisit.linkLine = line;

                    fuelRemaining -= Vector3.Distance(planet.transform.position, lastPlanet.transform.position);

                    // Draw circle around planet 
                    Destroy(distanceRingInstance);
                    distanceRingInstance = Instantiate(distanceRingPrefab);
                    distanceRingInstance.transform.position = new Vector3(planet.transform.position.x, planet.transform.position.y, 1015);
                    if (BuildModeController.Instance.furnitureTileOWWMap.ContainsKey("Fuel Tank"))
                    {
                        distanceRingInstance.GetComponent<DistanceRing>().xradius = fuelRemaining;
                        distanceRingInstance.GetComponent<DistanceRing>().yradius = fuelRemaining;
                        distanceRingInstance.GetComponent<DistanceRing>().CreatePoints();
                    }
                }
            }
        }
    }

    public void OnMapMode()
    {
        if (BuildModeController.Instance.furnitureTileOWWMap.ContainsKey("Fuel Tank"))
        {
            fuelRemaining = BuildModeController.Instance.furnitureTileOWWMap["Fuel Tank"].Count * 10;
            // Draw circle around planet 
            Destroy(distanceRingInstance);
            distanceRingInstance = Instantiate(distanceRingPrefab);
            distanceRingInstance.transform.position = new Vector3(earth.transform.position.x, earth.transform.position.y, 1015);

            distanceRingInstance.GetComponent<DistanceRing>().xradius = fuelRemaining;
            distanceRingInstance.GetComponent<DistanceRing>().yradius = fuelRemaining;
            distanceRingInstance.GetComponent<DistanceRing>().CreatePoints();
        }
    }

    public float GetShipSpeedCurrent()
    {
        return shipSpeedCurrent;
    }

    // Used by UI button 
    public void LaunchJourney()
    {
        if (nextPlanetVisit != null) 
        {
            Destroy(distanceRingInstance);
            shipCoordinates = earth.GetNextPlanet().GetPlanetCoordinates();
            //shipSpeedCurrent = shipSpeedMax;
            //TransitionController.Instance.StartTransitionToMain();
            StartCoroutine(TransitionController.Instance.TransitionLandingWithoutZoom());
            isJourneyEditMode = false;
            isAtOriginPlanet = true;
        }
        else
        {
            // Can't launch without setting a destination!! 
        }
    }

    public Vector2 GetShipCoordinates()
    {
        return shipCoordinates;
    }

    public Vector2 GetNextPlanetVist()
    {
        if (nextPlanetVisit != null) return nextPlanetVisit.transform.position;
        else return Vector2.zero;
    }

    // Called from UI Button after arrival
    public void Land()
    {
        if (TransitionController.Instance.isMapMode) StartCoroutine(TransitionController.Instance.TransitionLandingWithoutZoom());
        else StartCoroutine(TransitionController.Instance.TransitionLanding());
    }

    // Called when the above coroutine is completed
    public void OnLandComplete()
    {
        if (currentPlanetVisit != null)
        {
            if (currentPlanetVisit == earth)
            {
                EndJourney();
            }
            else SpawnPlanetInterfaceGO();

        }
        else
        {
            currentPlanetVisit = lastPlanetVisit;
            SpawnPlanetInterfaceGO();
        }
    }

    private void SpawnPlanetInterfaceGO()
    {
        GameObject planetInterfaceGO = Instantiate(PlanetUIPrefab);
        planetInterfaceGO.GetComponent<PlanetInterface>().SetMoveWithPlanet(false);

        planetInterfaceGO.transform.SetParent(UserInterfaceController.Instance.canvas_Main.transform);
        planetInterfaceGO.transform.localPosition = new Vector3(0, 50, 0);
        planetInterfaceGO.transform.localScale = Vector3.one;
        planetInterfaceGO.GetComponent<PlanetInterface>().SetPlanet(currentPlanetVisit, true);
        planetInterfaceGO.GetComponent<PlanetInterface>().button_AddToJourney.gameObject.SetActive(false);

        InputController.Instance.cameraZoomEnabled = false;
        planetInterfaceGO.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 600);

    }

    // After landing and boarding passengers 
    public void ContinueJourney() 
    {
        // Set the next destination
        UserInterfaceController.Instance.ShowMainUI();

        InputController.Instance.cameraZoomEnabled = true;

        if (!isAtOriginPlanet)
        {
            lastPlanetVisit.ClearLinkedPlanets();
            Destroy(nextPlanetVisit.linkLine);

            lastPlanetVisit = nextPlanetVisit;
            nextPlanetVisit = nextPlanetVisit.GetPreviousPlanet();
        }
        else isAtOriginPlanet = false;

        shipSpeedCurrent = shipSpeedMax;
        isJourneyEditMode = false;
        StartCoroutine(TransitionController.Instance.TransitionDeparture());

    }

    public void EndJourney()
    {
        lastPlanetVisit.ClearLinkedPlanets();
        earth.ClearLinkedPlanets();
        Destroy(earth.linkLine);
        nextPlanetVisit = null;
        lastPlanetVisit = null;

        isJourneyEditMode = true;
        StartCoroutine(TransitionController.Instance.TransitionInMap());

        foreach (GameObject passengerGO in currentPassengers)
        {
            int payment = passengerGO.GetComponent<Passenger>().GetPassengerFare();
            CurrencyController.Instance.ChangeBankBalance(payment);
            Destroy(passengerGO);

        }
        
    }

}
