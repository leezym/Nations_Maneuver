using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;


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
    public double saldo;

    public Resultados(double y, double inf, double saldo)
    {        
        this.y = y;
        this.inf = inf;
        this.saldo = saldo;
    }
}

public class WindowGraph : Singleton<WindowGraph>
{
    static Color color = Color.black;
    static int X_SIZE_BAR = 50;

    float graphWidth;
    float graphHeight;
    float yMaximum;
    float yMininum;

    [Header("UI Data")] 
    public TMP_Text currentPIB;
    public TMP_Text currentTInf;
    public TMP_Text currentSaldo;

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

    void Start()
    {
        yMaximum = 0;
        yMininum = 0;
        graphWidth = graphContainer.rect.width - 120; // Margen para que el label se vea
        graphHeight = graphContainer.rect.height;
    }

    public void ContinueGame()
    {
        GetLocalData();
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

    // value: 0 = variación PIB, 1 = tasa inflación, otro = balance fiscal
    static double GetResultValue(Resultados resultado, int value)
    {
        return value == 0 ? resultado.y : value == 1 ? resultado.inf : resultado.saldo;
    }

    public void CreateGraph(int value)
    {
        DeleteGraph();

        if(shiftsList.Count > 0)
        {
            //valores maximo y minimo de los turnos actuales, siempre incluyendo el 0
            double max = double.MinValue, min = double.MaxValue;
            foreach (Turnos turno in shiftsList)
            {
                double v = GetResultValue(turno.resultados, value);
                if (v > max) max = v;
                if (v < min) min = v;
            }

            yMaximum = Mathf.Max((float)max, 0);
            yMininum = Mathf.Min((float)min, 0);
        }

        float yRange = yMaximum - yMininum;
        float zeroPosition = (0 - yMininum / yRange) * graphHeight; 
        zeroLine.anchoredPosition = new Vector2(0, zeroPosition);

        Vector2? lastCirclePosition = null;
        float xStep = graphWidth / GameLoadManager.SHIFTS;

        for (int i = 0; i < shiftsList.Count; i++) {
            double yValue = GetResultValue(shiftsList[i].resultados, value);

            float xPosition = (i + 1) * xStep;
            float yPosition = (((float)yValue - yMininum) / yRange) * graphHeight;
            Vector2 circlePosition = new Vector2(xPosition, yPosition);

            CreateBar(value, xPosition, yPosition, zeroPosition, yValue);
            resultGameObjects.Add(CreateCircle(circlePosition));

            if (lastCirclePosition.HasValue)
                resultGameObjects.Add(CreateDotConnection(lastCirclePosition.Value, circlePosition));

            lastCirclePosition = circlePosition;
        }       

        zeroLine.transform.SetAsLastSibling();
    }

    // Crea un GameObject UI con Image como hijo del contenedor de la gráfica, anclado abajo a la izquierda
    private RectTransform CreateGraphImage(string name, Sprite sprite, Color imageColor, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject gameObject = new GameObject(name, typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);

        Image image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = imageColor;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;

        return rectTransform;
    }

    private void CreateBar(int value, float xPosition, float yPosition, float zeroPosition, double yValue)
    {
        GameObject gameObjectLabel = Instantiate(label, new Vector2(xPosition, yPosition), transform.rotation);
        gameObjectLabel.GetComponentInChildren<TMP_Text>().text = yValue.ToString("F2");
        gameObjectLabel.transform.SetParent(graphContainer, false);
        gameObjectLabel.transform.SetParent(graphGameObject, true);

        // La barra crece desde la línea del cero hacia el valor
        float ySize = Mathf.Abs(yPosition - zeroPosition);
        float yBase = Mathf.Min(yPosition, zeroPosition);

        RectTransform bar = CreateGraphImage("bar", barSprites[value], Color.white, new Vector2(xPosition, yBase), new Vector2(X_SIZE_BAR, ySize));
        bar.pivot = new Vector2(0.5f, 0);

        Button button = bar.gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => gameObjectLabel.SetActive(!gameObjectLabel.activeSelf));

        resultGameObjects.Add(bar.gameObject);
        resultGameObjects.Add(gameObjectLabel);
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        return CreateGraphImage("circle", circleSprite, color, anchoredPosition, new Vector2(15, 15)).gameObject;
    }

    private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB) {
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        RectTransform rectTransform = CreateGraphImage("dotConnection", null, color, dotPositionA + dir * distance * .5f, new Vector2(distance, 3f));
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));

        return rectTransform.gameObject;
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
        foreach (GameObject resultGameObject in resultGameObjects)
            Destroy(resultGameObject);
        resultGameObjects.Clear();
    }
}
