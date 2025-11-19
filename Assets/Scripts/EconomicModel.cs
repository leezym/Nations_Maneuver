using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;

public class EconomicModel : MonoBehaviour
{
    public static EconomicModel Instance {get; private set;}
    
    bool finishedGame => GameLoadManager.Instance.GetFinishedGame();
    List<Turnos> shiftsList => WindowGraph.Instance.shiftsList;

    int shift;
    public int GetShift() { return shift; }
    public void SetShift(int shift) { this.shift = shift; }

    InputValidations inputValidations;

    [Header("Variables exógenas")]
    [Header("Mercado de bienes")]
    public static double C = 160; // Consumo autónomo //// == cambio en las expectativas de los consumidores.
    public static double I = 100; // inversión autónoma ////== Cambio de expectativas sobre el recimiento económico

    [Header("Oferta")]
    public static double w = 50; // Salario nominal ////== Sindicatos consiguieron un incremento salaria.
    public static double ka = 30000; // stock de capital ////== desatre natural disminuye el stock de capital
    public static double L = 225; // trabajo //// == Pandemia mata el 25% se la población.
    public static double A = 1; // tecnología //// == Una mejora en la tecnología, aumenta la productividad


    [Header("Parámetros")]
    [Header("Mercado de bienes")]
    public static double c = 0.4; // PMgC este parámetro está entre cero y uno 0<c<1.
    public static double b = 1500; // sensibilidad de la inversión a la tasa de interés.

    [Header("Mercado dinero")]
    public static double k = 0.40; // sensibilidad de la demanda de dinero a la renta
    public static double h = 500; // sensibilidad de la demanda de dinero a cambios en la tasa de interés

    [Header("Oferta")]
    public static double alpha = 0.6; // proporción de la utilización del capital en la producción
    
    // Parameters for DefineSimplifications
    [HideInInspector]
    public static double omega, gamma, q, u;

    [Header("UI TMP_Text")]
    public TMP_Text shiftText;
    public TMP_Text shiftText2;

    [Header("UI TMP_InputField")]
    public TMP_InputField textGasto;
    public TMP_InputField textTasaImp;
    public TMP_InputField textOma;
    public TMP_InputField textPIB;
    public TMP_InputField textInflacion;
    public TMP_InputField textBalance;

    [Header("UI Toggle")]
    public Toggle toggleCompra;
    public Toggle toggleVenta;

    [Header("UI UI_Screen")]
    public UI_Screen resultadosScreen;

    [Header("UI Button")]
    public Button continuarPartidaButton;
    public Button calcularButton;
    public Button siguienteTurnoButton;
    public Button finalizarPartidaButton;

    [Header("Variables de política. Variables de decisión del jugador (las que él maneja)")]
    // Instrumentos de política fiscal
    public static double G;
    public double t;
    // Instrumentos de política monetaria
    public static double M;

    const double EPS = 1e-6;

    [HideInInspector]
    public double y, r, p;
    [HideInInspector]
    public double variacionPIB_real, inf, saldo;
    [HideInInspector]
    public static double[] initialGuess;
    public double[] finalGuess, solutionVector;
    
    void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;    
    }

    void Start()
    {
        inputValidations = InputValidations.Instance;
    }

    public void SaveLocalData()
    {
        PlayerPrefs.SetInt("shift", shift);

        PlayerPrefs.SetString("G", textGasto.text); // gasto
        PlayerPrefs.SetString("t", textTasaImp.text); // tasa impositiva
        PlayerPrefs.SetString("M", textOma.text); // bonos

        string formattedInitialGuess = "";

        for (int i = 0; i < initialGuess.Length; i++)
        {
            formattedInitialGuess += initialGuess[i].ToString().Replace(",", ".");
            if (i < initialGuess.Length - 1)
            {
                formattedInitialGuess += "-";
            }
        }

        PlayerPrefs.SetString("initialGuess", formattedInitialGuess);
    }

    //en la ronda incial (turno 0), se asignan valores aleatorios en cada variable: G, t, M, initialGuess (y,r,p) -> (PIB, tasa interes, precio)
    //se ejecuta la solucion y esos resultados aleatorios me generan un nuevo resultado para comenzar el turno 1: PIB, tasa interes, p, tasa inflacion y balance fiscal
    //los rangos de los valores varian de donde empiece el jugador
    // G = {50 - 1000} ++50
    // t = {0 - 1} ++0.1
    // M = {5 - 40} ++5

    private (double G, double t, double M) GenerateRandomPolicy()
    {
        double Grand = inputValidations.limInfGasto + UnityEngine.Random.Range(1, 21) * inputValidations.stepGasto;
        double trand = inputValidations.limInfTasaImp + (int)UnityEngine.Random.Range(1, 11) * inputValidations.stepTasaImp;
        double Mrand = inputValidations.limInfOma   + UnityEngine.Random.Range(1, 9)  * inputValidations.stepOma;

        return (Grand, trand, Mrand);
    }

    public void NewGame()
    {
        shift = 1;

        calcularButton.gameObject.SetActive(true);
        siguienteTurnoButton.gameObject.SetActive(true);
        finalizarPartidaButton.gameObject.SetActive(false);

        // Aleatorio por primera vez
        (G, t, M) = GenerateRandomPolicy();
        /*G = 400;
        t = 0.5;
        M = 40;*/
        DefineSimplifications(G, t, M);
        initialGuess = new double[] { 510, 0.3, 2.5 };
        finalGuess = SolveEquations();
        CalculateReport();

        // Aleatorio por segunda vez
        (G, t, M) = GenerateRandomPolicy();
        /*G = 400;
        t = 0.5;
        M = 35;*/
        DefineSimplifications(G, t, M);
        finalGuess = SolveEquations();
        CalculateReport();

        GenerateReport();

        NotificationsManager.Instance.WarningNotifications(
            "Los nuevos valores son:\nVariación PIB real: " + variacionPIB_real.ToString("F2") +
            "\nTasa inflación: " + inf.ToString("F2") +
            "\nBalance fiscal: " + saldo.ToString("F2") +
            "\nGasto público: " + G.ToString("F2") +
            "\nTasa impositiva: " + t.ToString("F2")
        );
    }

    public void ContinueGame()
    {
        string[] formattedInitialGuess = PlayerPrefs.GetString("initialGuess").Split('-');
        initialGuess = new double[formattedInitialGuess.Length];

        for (int i = 0; i < formattedInitialGuess.Length; i++)
        {
            string cleanedValue = formattedInitialGuess[i].Replace(",", ".");
            initialGuess[i] = double.Parse(cleanedValue, System.Globalization.CultureInfo.InvariantCulture);
        }        

        if(shift <= GameLoadManager.SHIFTS)
        {
            calcularButton.gameObject.SetActive(true);
            siguienteTurnoButton.gameObject.SetActive(true);
            finalizarPartidaButton.gameObject.SetActive(false);
        }
        else
        {
            calcularButton.gameObject.SetActive(false);
            siguienteTurnoButton.gameObject.SetActive(false);
            finalizarPartidaButton.gameObject.SetActive(true);
            finalizarPartidaButton.interactable = finishedGame ? false : true;          
        }

        if(shift > 1)
        {
            textGasto.text = PlayerPrefs.GetString("G"); // gasto
            textTasaImp.text = PlayerPrefs.GetString("t"); // tasa impositiva
            textOma.text = PlayerPrefs.GetString("M"); // bonos
        }
    }   

    public void FinishedGame()
    {
        GameLoadManager.Instance.SetFinishedGame(true);

        NotificationsManager.Instance.QuestionNotifications("¿Quieres finalizar la partida?");
        NotificationsManager.Instance.SetYesButton(() =>
        {
            textGasto.text = "";
            textTasaImp.text = "";
            textOma.text = "";
            toggleCompra.isOn = false;
            toggleVenta.isOn = false;
            finalizarPartidaButton.interactable = false;
        });
    }

    public void Calculate()
    {
        if(!string.IsNullOrEmpty(textGasto.text) && !string.IsNullOrEmpty(textTasaImp.text) && !string.IsNullOrEmpty(textOma.text) && (toggleCompra.isOn || toggleVenta.isOn))
        {
            NotificationsManager.Instance.QuestionNotifications("¿Quieres calcular?");
            NotificationsManager.Instance.SetYesButton(()=> {
                if(shift == GameLoadManager.SHIFTS)
                {
                    calcularButton.gameObject.SetActive(false);
                    siguienteTurnoButton.gameObject.SetActive(false);
                    finalizarPartidaButton.gameObject.SetActive(true);
                }

                G = Convert.ToDouble(textGasto.text); // gasto
                t = Convert.ToDouble(textTasaImp.text); // tasa impositiva
                M = (toggleCompra.isOn ? M + Convert.ToDouble(textOma.text) : M - Convert.ToDouble(textOma.text)); // bonos

                DefineSimplifications(G, t, M);
                finalGuess = SolveEquations();
                CalculateReport();
                GenerateReport();

                UI_System.Instance.SwitchScreens(resultadosScreen);
                shift+=1;
            });

        }
        else
            NotificationsManager.Instance.WarningNotifications("¡Llene todos los campos para calcular!");
    }

    public void DefineSimplifications(double G, double t, double M)
    {
        gamma = (1 - c * (1 - t));

        q = 1 / alpha;
        u = (1 - alpha) / alpha;
    }

    static void Doxn(double[] vars, double[] f)
    {
        double y = vars[0];
        double r = vars[1];
        double p = vars[2];

        // MISMO clipping que en Python
        p = (p < EPS) ? EPS : p;
        r = (r < EPS) ? EPS : r;

        double da = gamma * y - C - I - G + b * r;
        double m  = M / p - k * y + h * r;
        double oa = y - (Math.Pow(A, q) * ka * Math.Pow(1 - alpha, u) * Math.Pow(p / w, u));

        f[0] = da;
        f[1] = m;
        f[2] = oa;
    }

    public static double[] SolveEquations()
    {
        double[] x = (double[])initialGuess.Clone();
        int it;

        bool ok = MinpackLikeSolver.Solve(
            Doxn,
            x,
            maxIterations: 200,
            tol: 1e-12,
            lambda0: 1e-3,
            stepRel: 1e-7,
            out it
        );

        if (!ok)
            Debug.LogWarning("No convergió");

        return x;
    }

    public void CalculateReport()
    {
        double yInicial = initialGuess[0];
        double yFinal = finalGuess[0];

        double pInicial = initialGuess[2];
        double pFinal = finalGuess[2];
        
        double PIB_realInicial = yInicial / pInicial;
        double PIB_realFinal = yFinal / pFinal;
        variacionPIB_real = ((PIB_realFinal - PIB_realInicial) / PIB_realInicial) * 100; // tasa de crecimiento del PIB_real ( )= [(PIB_real_1 - PIB_real_0)/ PIB_real_0 ]*100

        // ingreso del gobierno Nominal
        double ingreso = t * yFinal;
        saldo = ingreso - G; // balance fiscal

        inf = ((pFinal - pInicial) / pInicial) * 100; // Inflación (\pi) = [(p_1 -p_0)/p_0 ]* 100
        
        initialGuess = finalGuess;

        Debug.Log("G: "+G);
        Debug.Log("t: "+t);
        Debug.Log("M: "+M);

        Debug.Log("PIB: "+yFinal);
        Debug.Log("PIB real: "+yFinal/pFinal);
        Debug.Log("Precios: "+pFinal);
        Debug.Log("Inflación: "+inf);
    }

    public void GenerateReport() 
    {
        textPIB.text = variacionPIB_real.ToString("F2"); // variacion PIB real
        textInflacion.text = inf.ToString("F2"); // tasa inflacion
        textBalance.text = saldo.ToString("F2"); // balance fiscal

        WindowGraph.Instance.shiftsList.Add(new Turnos(new Datos(G, t, M), new Resultados(variacionPIB_real, inf, saldo)));
    }
}