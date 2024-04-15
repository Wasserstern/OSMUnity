using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;

public class OSMRenderer : MonoBehaviour
{
    // Various settings
    public bool useManualInput; 
    public bool searchCircular;
    public float locationLongitude;
    public float locationLatitude;
    public float boundingBoxOffset;
    public float circularSearchRadius;
    public GameObject wayPrefab;
    public GameObject cameraContainerPrefab;
    public Vector4 bboxInput;
    [TextArea(15, 20)]
    public string query;
    public string url;
    public float mapWidth;
    [SerializeField]
    bool isRequesting;
    public float planetRadius;
    public float camZOffset;
    public float camMoveTimeInSeconds;
    public float zoomSpeed;
    
    string originalQuery;
    Transform currentCameraTransform;
    void Start()
    {
        originalQuery = query;
    }

    void OnGUI()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold,
        };
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.cyan;
        if(GUI.Button(new Rect(Screen.width / 2 -100, 20, 200, 100), "Get map data", buttonStyle) && !isRequesting)
        {
            isRequesting = true;
            StartCoroutine(OverpassPost());
        }
    }

    void Update()
    {
        if(!isRequesting && Input.GetKeyDown(KeyCode.O)){
            isRequesting = true;
            StartCoroutine(OverpassPost());
        }
        if(currentCameraTransform != null){
            currentCameraTransform.localPosition = currentCameraTransform.localPosition + new Vector3(0f, 0f, Input.mouseScrollDelta.y *zoomSpeed);
        }
    }
    IEnumerator OverpassPost()
    {
        GameObject previousWayContainer = GameObject.Find("WayContainer");
        if(previousWayContainer != null){
            Destroy(previousWayContainer);
        }

        // Overpass query creation. {}-markers in the original inspectory query are replaced by inspector settings.
        query = originalQuery;
        float minLat = 0;
        float maxLat = 0;
        float minLon = 0;
        float maxLon = 0;

        if(!useManualInput)
        {
            minLat = locationLatitude - boundingBoxOffset;
            maxLat = locationLatitude + boundingBoxOffset;
            minLon = locationLongitude - boundingBoxOffset / 2;
            maxLon = locationLongitude + boundingBoxOffset / 2;
        }
        else{
            minLon = bboxInput[1];
            minLat = bboxInput[0];
            maxLon = bboxInput[3];
            maxLat = bboxInput[2];
        }
        CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
        if(searchCircular)
        {
            query = query.Replace("{circular}", "(around:" + circularSearchRadius.ToString(culture) + "," + locationLatitude.ToString(culture) + "," + locationLongitude.ToString(culture) + ")" );
        }
        else
        {
            query = query.Replace("{circular}", "");
        }

        query = query.Replace("{minLat}", minLat.ToString(culture));
        query = query.Replace("{maxLat}", maxLat.ToString(culture));
        query = query.Replace("{minLon}", minLon.ToString(culture));
        query = query.Replace("{maxLon}", maxLon.ToString(culture));

        // Sending a POST request via the overpass API.

        UnityWebRequest uww = UnityWebRequest.Post(url, query, "application/json");
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        uww.downloadHandler = downloadHandler;

        Debug.Log("Starting Overpass request.");
        yield return uww.SendWebRequest();

        if(uww.result != UnityWebRequest.Result.Success)
        {

            Debug.Log(uww.error);
        }
        else{
            // 
            StringReader reader = new StringReader(uww.downloadHandler.text);
            XmlSerializer serializer = new XmlSerializer(typeof(Osm));
            Osm osm = (Osm)serializer.Deserialize(reader);
            GameObject wayContainer = new GameObject("WayContainer");
            foreach(Way way in osm.Ways)
            {
                bool isBuilding = false;
                foreach(Tag tag in way.Tags)
                {
                    if(tag.K == "building"){
                        isBuilding = true;
                        break;
                    }
                }
                if(!isBuilding)
                {
                    // Way element is not tagged as building. Render as normal street.
                    GameObject wayObject = GameObject.Instantiate(wayPrefab, wayContainer.transform);
                    LineRenderer wayRenderer = wayObject.GetComponent<LineRenderer>();
                    wayRenderer.positionCount = way.Nodes.Count;
                    for(int i = 0; i < way.Nodes.Count; i++)
                    {
                        Node node = way.Nodes[i];

                        // For cartesian coordinates
                        /*
                        float xCoord = (float)( 360f * (180f + node.Lon));
                        float yCoord = (float)( 180f * (90f - node.Lat));
                        float zCoord = 0f;
                        */

                        // For spherical representation
                        float xCoord = planetRadius * Mathf.Cos((float)node.Lat * Mathf.PI / 180f) * Mathf.Cos((float)node.Lon * Mathf.PI / 180f);
                        float yCoord = planetRadius * Mathf.Cos((float)node.Lat * Mathf.PI / 180f) * Mathf.Sin((float)node.Lon * Mathf.PI / 180f);
                        float zCoord = planetRadius * Mathf.Sin((float)node.Lat * Mathf.PI / 180f); 
                        wayRenderer.SetPosition(i, new Vector3(xCoord, yCoord, zCoord));
                    }
                }
                else
                {
                    
                    // Way element is tagged as building. Render building mesh.
                    // TODO: This is not finished. Hint: Look up mesh triangulation.
                    GameObject wayObject = GameObject.Instantiate(wayPrefab, wayContainer.transform);
                    wayObject.GetComponent<LineRenderer>().enabled = false;
                    MeshRenderer meshRenderer = wayObject.AddComponent<MeshRenderer>();
                    MeshFilter meshFilter = wayObject.AddComponent<MeshFilter>();
                    Vector3[] vertices = new Vector3[way.Nodes.Count];
                    Vector3[] uvs = new Vector3[way.Nodes.Count];
                    
                }

                
            }
            GameObject mainCamera = GameObject.Instantiate(cameraContainerPrefab, wayContainer.transform);
            float xCam = planetRadius * Mathf.Cos(locationLatitude * Mathf.PI / 180f) * Mathf.Cos(locationLongitude * Mathf.PI / 180f);
            float yCam = planetRadius * Mathf.Cos(locationLatitude * Mathf.PI / 180f) * Mathf.Sin(locationLongitude * Mathf.PI / 180f);
            float zCam = planetRadius * Mathf.Sin(locationLatitude * Mathf.PI / 180f); 
            mainCamera.transform.position = new Vector3(xCam, yCam, zCam);
            mainCamera.transform.LookAt(mainCamera.transform.parent);
            currentCameraTransform = mainCamera.transform.GetChild(0);
            StartCoroutine(MoveCam());
        }
        isRequesting = false;
       

    }

    // Initial cam zoom after succesfull rendering 

    private IEnumerator MoveCam()
    {
        float startTime = Time.time;
        float elapsedTime = 0f;
        Vector3 camCurrentPosition = currentCameraTransform.localPosition;
        Vector3 camTargetPosition = new Vector3(currentCameraTransform.localPosition.x, currentCameraTransform.localPosition.y, -camZOffset);
        while(Time.time - startTime < camMoveTimeInSeconds){
            float t = elapsedTime / camMoveTimeInSeconds;
            t = EaseFunctions.easeInCubic(t);
            currentCameraTransform.localPosition = Vector3.Lerp(camCurrentPosition, camTargetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // XML Parsing for OSM data

    [XmlRoot(ElementName="meta")]
    public class Meta { 

        [XmlAttribute(AttributeName="osm_base")] 
        public DateTime OsmBase { get; set; } 
    }

    [XmlRoot(ElementName="bounds")]
    public class Bounds { 

        [XmlAttribute(AttributeName="minlat")] 
        public double Minlat { get; set; } 

        [XmlAttribute(AttributeName="minlon")] 
        public double Minlon { get; set; } 

        [XmlAttribute(AttributeName="maxlat")] 
        public double Maxlat { get; set; } 

        [XmlAttribute(AttributeName="maxlon")] 
        public double Maxlon { get; set; } 
    }

    [XmlRoot(ElementName="nd")]
    public class Node { 

        [XmlAttribute(AttributeName="ref")] 
        public string Ref { get; set; } 
        [XmlAttribute(AttributeName="lat")]
        public double Lat {get; set;}
        [XmlAttribute(AttributeName="lon")]
        public double Lon {get; set;}
    }

    [XmlRoot(ElementName="tag")]
    public class Tag { 

        [XmlAttribute(AttributeName="k")] 
        public string K { get; set; } 

        [XmlAttribute(AttributeName="v")] 
        public string V { get; set; } 
    }

    [XmlRoot(ElementName="way")]
    public class Way { 

        [XmlElement(ElementName="nd")] 
        public List<Node> Nodes { get; set; } 

        [XmlElement(ElementName="tag")] 
        public List<Tag> Tags { get; set; } 

        [XmlAttribute(AttributeName="id")] 
        public string Id { get; set; } 
    }

    [XmlRoot(ElementName="osm")]
    public class Osm { 

        [XmlElement(ElementName="note")] 
        public string Note { get; set; } 

        [XmlElement(ElementName="meta")] 
        public Meta Meta { get; set; } 

        [XmlElement(ElementName="bounds")] 
        public Bounds Bounds { get; set; } 

        [XmlElement(ElementName="way")] 
        public List<Way> Ways { get; set; } 

        [XmlAttribute(AttributeName="version")] 
        public double Version { get; set; } 

        [XmlAttribute(AttributeName="generator")] 
        public string Generator { get; set; } 

        [XmlText] 
        public string Text { get; set; } 
    }



}
