using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.EventSystems;


[Serializable]
public class Turnos
{
    public Datos datos;
    public Resultados resultados;

    public Turnos(Datos datos, Resultados resultados)
    {        
        this.datos = datos;
        this.resultados = resultados;
    }
}

[Serializable]
public class Datos
{
    public double G;
    public double t;
    public double M;

    public Datos(double G, double t, double M)
    {        
        this.G = G;
        this.t = t;
        this.M = M;
    }
}

[Serializable]
public class Resultados
{
    public double y;
    public double inf;
    public double BF;

    public Resultados(double y, double inf, double BF)
    {        
        this.y = y;
        this.inf = inf;
        this.BF = BF;
    }
}

public class WindowGraph : MonoBehaviour
{
    public static WindowGraph Instance {get; private set;}
    static Color color = Color.black;
    float graphWidth;
    float graphHeight;
    float yMaximum;
    float yMininum;


    [Header("UI Graph")]    
    public Button defaultButton;
    public RectTransform graphGameObject;
    public RectTransform axisXContainer;
    public RectTransform graphContainer;

    [Header("UI Graph Container")]
    public RectTransform zeroLine;
    public GameObject label;

    [Header("UI Graph Items")]
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Sprite[] barSprites;
    
    [HideInInspector]
    public List<Turnos> shiftsList;
    List<GameObject> resultGameObjects = new List<GameObject>();
                        
    public void ShowDefaultGraph() => defaultButton.onClick.Invoke();

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;    
    }

    void Start()
    {
        yMaximum = 0;
        yMininum = 0;
        graphWidth = graphContainer.rect.width - 120; // Margen para que el label se vea
        graphHeight = graphContainer.rect.height;
    }

    public void SaveLocalData()
    {
        string jsonShiftsList = JsonConvert.SerializeObject(shiftsList);
        PlayerPrefs.SetString("shiftsList", jsonShiftsList);
    }

    public void GetLocalData()
    {
        string jsonShiftsList = PlayerPrefs.GetString("shiftsList");
        shiftsList = JsonConvert.DeserializeObject<List<Turnos>>(jsonShiftsList);
    }

    public void SetHeight(Transform t)
    {
        Vector3 scale = t.localScale;
        scale.y = 117;
        t.localScale = scale;
    }

    public void CreateGraph(int value)
    {
        DeleteGraph();
        GameObject lastCircleGameObject = null;
               
        if(shiftsList.Count > 0)
        {
            yMaximum = (float)shiftsList.Max(turno => (value == 0 ? turno.resultados.y : value == 1 ? turno.resultados.inf : turno.resultados.BF)); //valor maximo de los turnos actuales
            yMininum = (float)shiftsList.Min(turno => (value == 0 ? turno.resultados.y : value == 1 ? turno.resultados.inf : turno.resultados.BF)); //valor minimo de los turnos actuales

            yMaximum = (yMaximum > 0 ? yMaximum : 0);
            yMininum = (yMininum < 0 ? yMininum : 0);
        }

        float zeroPosition = (0 - yMininum / (yMaximum - yMininum)) * graphHeight; 
        zeroLine.anchoredPosition = new Vector2(0, zeroPosition);

        for (int i = 0; i < shiftsList.Count; i++) {
            Resultados resultado = shiftsList[i].resultados;

            double yValue = (value == 0 ? resultado.y : value == 1 ? resultado.inf : resultado.BF);

            float xPosition = (i + 1) * (graphWidth / shiftsList.Count);
            float yPosition = (((float)yValue - yMininum) / (yMaximum - yMininum)) * graphHeight;
           
            GameObject[] barGameObject = CreateBar(value, 0, xPosition, yPosition, zeroPosition, yValue);
            resultGameObjects.Add(barGameObject[0]);
            resultGameObjects.Add(barGameObject[1]);

            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            resultGameObjects.Add(circleGameObject);

            GameObject numberGameObject = CreateNumber(i+1, xPosition);
            resultGameObjects.Add(numberGameObject);

            if (lastCircleGameObject != null) {
                GameObject dotConnection = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
                resultGameObjects.Add(dotConnection);
            }
            lastCircleGameObject = circleGameObject;
        }       

        zeroLine.transform.SetAsLastSibling();
    }

    private GameObject[] CreateBar(int value, float ySize, float xPosition, float yPosition, float zeroPosition, double yValue)
    {
        GameObject gameObjectLabel = Instantiate(label, new Vector2(xPosition, yPosition), transform.rotation);
        gameObjectLabel.GetComponentInChildren<TMP_Text>().text = yValue.ToString("F2");
        gameObjectLabel.transform.SetParent(graphContainer, false);
        gameObjectLabel.transform.SetParent(graphGameObject, true);

        
        if(yPosition > zeroPosition)
        {
            ySize = yPosition - zeroPosition;
            yPosition = zeroPosition;
        }
        else if(yPosition < zeroPosition)
        {
            ySize = zeroPosition - yPosition;
        }
        
        GameObject gameObjectBar = new GameObject("bar", typeof(Image));
        gameObjectBar.transform.SetParent(graphContainer, false);
        gameObjectBar.GetComponent<Image>().sprite = barSprites[value];
        
        RectTransform rectTransform = gameObjectBar.GetComponent<RectTransform>();        
        rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
        rectTransform.sizeDelta = new Vector2(30, ySize);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;

        EventTrigger eventTrigger = gameObjectBar.AddComponent<EventTrigger>();
        
        EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
        entryPointerEnter.eventID = EventTriggerType.PointerEnter;
        entryPointerEnter.callback.AddListener((data) => { gameObjectLabel.SetActive(true); });
        eventTrigger.triggers.Add(entryPointerEnter);

        EventTrigger.Entry entryPointerExit = new EventTrigger.Entry();
        entryPointerExit.eventID = EventTriggerType.PointerExit;
        entryPointerExit.callback.AddListener((data) => { gameObjectLabel.SetActive(false); });
        eventTrigger.triggers.Add(entryPointerExit);

        return new GameObject[] {gameObjectBar, gameObjectLabel};
    }

    private GameObject CreateNumber(int value, float xPosition)
    {
        GameObject gameObject = new GameObject("number");
        gameObject.transform.SetParent(axisXContainer, false);

        TextMeshProUGUI textComponent = gameObject.AddComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.text = value.ToString();
        textComponent.fontSize = 25;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(xPosition, 25);
        rectTransform.sizeDelta = new Vector2(30, 30);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;

        return gameObject;
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        gameObject.GetComponent<Image>().color = color;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(15, 15);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;

        return gameObject;
    }

    private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB) {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = color;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        Vector2 dir = (dotPositionB - dotPositionA).normalized;

        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        rectTransform.sizeDelta = new Vector2(distance, 3f); 
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));

        return gameObject;

    }

    private float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }

    public void DeleteGraph()
    {
        for(int i = 0; i < resultGameObjects.Count; i++)
        {
            Destroy(resultGameObjects[i]);
        }
    }    
}
