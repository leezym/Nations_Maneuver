using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Numerics.RootFinding;
using TMPro;

public class EconomicModel : MonoBehaviour
{
    public static EconomicModel Instance {get; private set;}
    
    bool finishedGame => GameLoadManager.Instance.GetFinishedGame();
    List<Turnos> shiftsList => WindowGraph.Instance.shiftsList;

    int shift;
    public int GetShift() { return shift; }
    public void SetShift(int shift) { this.shift = shift; }

    InputValidations inputValidations;

    // Parameters
    [HideInInspector]
    public double a = 160;
    [HideInInspector]
    public double f = 100;
    [HideInInspector]
    public double w = 50;
    [HideInInspector]
    public double ka = 30000;
    [HideInInspector]
    public double L = 225;
    [HideInInspector]
    public double A = 1;
    [HideInInspector]
    public double x1 = 0.2;
    [HideInInspector]
    public double x2 = 30;
    [HideInInspector]
    public double m1 = 0.06;
    [HideInInspector]
    public double m2 = 10;
    [HideInInspector]
    public double y_ex = 20000;
    [HideInInspector]
    public double r_ex = 0.05;
    [HideInInspector]
    public double c = 0.6;
    [HideInInspector]
    public double b = 1500;
    [HideInInspector]
    public double k = 0.2;
    [HideInInspector]
    public double h = 1000;
    [HideInInspector]
    public double alpha = 0.5;
    [HideInInspector]
    public double rho = 10;
    [HideInInspector]
    public double e_0 = 2;

    // Parameters for DefineSimplifications
    [HideInInspector]
    public double omega, gamma, q, u, pi_e, sigma;

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

    // Variables del juego
    [HideInInspector]
    public double G, t, M;
    [HideInInspector]
    public double y, r, p;
    [HideInInspector]
    public double variacionPIB, inf, BF;
    [HideInInspector]
    public double[] initialGuess, finalGuess, solutionVector;
    
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
        PlayerPrefs.SetString("t", textTasaImp.text ); // tasa impositiva
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
    // G = {50 - 500} ++50
    // t = {0.05 - 0.25} ++0.02
    // M = {4000 - 5500} ++150

    public void NewGame()
    {
        GameLoadManager.Instance.SetFinishedGame(false);

        shift = 1;
        initialGuess = new double[] { 800, 0.05, 1.1 }; // valores iniciales (y,r,p)

        calcularButton.gameObject.SetActive(true);
        siguienteTurnoButton.gameObject.SetActive(true);
        finalizarPartidaButton.gameObject.SetActive(false);

        // Aleatorio por primera vez
        G = UnityEngine.Random.Range(1, 11) * 50; // gasto
        t = inputValidations.limInfTasaImp + UnityEngine.Random.Range(0, 11) * inputValidations.stepTasaImp; // tasa impositiva
        M = inputValidations.limInfOma + UnityEngine.Random.Range(0, 10) * inputValidations.stepOma; // bonos

        DefineSimplifications(G, t, M);
        finalGuess = SolveEquations(initialGuess);
        initialGuess = finalGuess;
    }

    public void ContinueGame()
    {
        WindowGraph.Instance.GetLocalData();

        GameLoadManager.Instance.SetFinishedGame(PlayerPrefs.GetInt("finishedGame") == 1 ? true : false);

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
        if(textGasto.text != "" && textTasaImp.text != "" && textOma.text != "" && (toggleCompra.isOn || toggleVenta.isOn))
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
                finalGuess = SolveEquations(initialGuess);
                GenerateReport();
                
                UI_System.Instance.SwitchScreens(resultadosScreen);
                SetShift(shift+1);
            });

        }
        else
            NotificationsManager.Instance.WarningNotifications("¡Llene todos los campos para calcular!");
    }

    public void DefineSimplifications(double G, double t, double M)
    {
        omega = a + f + G + x1 * y_ex;
        gamma = 1 - c * (1 - t) + m1;
        q = 1 / alpha;
        u = (1 - alpha) / alpha;
        pi_e = 0;
        sigma = x2 - m2;
    }

    public double[] SolveEquations(double[] initialGuess)
    {        
        Func<double[], double[]> equations = vars =>
        {
            y = vars[0];
            r = vars[1];
            p = vars[2];

            double da = gamma * y + sigma * rho * r - omega - sigma * e_0 - sigma * rho * r_ex;
            double m = k * y - h * r - M / p - h * pi_e;
            double oa = y - ((Math.Pow(A, q)) * ka * (Math.Pow(1 - alpha, u)) * (Math.Pow(p/w, u)));

            return new double[] { da, m, oa };
        };

        double accuracy = 1e-8; // Desired accuracy
        int maxIterations = 100; // Maximum number of iterations
        double jacobianStepSize = 1e-4; // Step size for numerical Jacobian approximation

        solutionVector = Broyden.FindRoot(equations, initialGuess, accuracy, maxIterations, jacobianStepSize); // fsolve de python

        return solutionVector;
    }

    public void CalculateReport()
    {
        // finalGuess = {y, r, p}
        y = finalGuess[0]; // PIB
        r = finalGuess[1]; // tasa interes

        double yFinal = finalGuess[0]; // PIB final
        double pFinal = finalGuess[2]; // precio final

        double yInicial = initialGuess[0]; // PIB inicial
        double pInicial = initialGuess[2];  // precio inicial

        variacionPIB = ((yFinal - yInicial) / yInicial) * 100; // variacion PIB
        inf = ((pFinal - pInicial) / pInicial) * 100; // inflacion
        BF = (t * y) - G; // balance fiscal
    }

    public void GenerateReport() 
    {
        CalculateReport();

        textPIB.text = variacionPIB.ToString("F2"); // ya no es PIB sino su variacion
        textInflacion.text = inf.ToString("F2"); // tasa inflacion
        textBalance.text = BF.ToString("F2"); // balance fiscal

        initialGuess = finalGuess;
        WindowGraph.Instance.shiftsList.Add(new Turnos(new Datos(G, t, M), new Resultados(variacionPIB, inf, BF)));
    }
}