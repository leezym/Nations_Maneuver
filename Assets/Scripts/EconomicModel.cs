using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Numerics;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.RootFinding;
using TMPro;

public class EconomicModel : MonoBehaviour
{

    public static EconomicModel Instance {get; private set;}
    List<Turnos> shiftsList => WindowGraph.Instance.shiftsList;
    bool finishedGame => GameLoadManager.Instance.finishedGame;

    // Parameters
    double a = 160;
    double f = 100;
    double w = 50;
    double ka = 30000;
    double L = 225;
    double A = 1;
    double x1 = 0.2;
    double x2 = 30;
    double m1 = 0.06;
    double m2 = 10;
    double y_ex = 20000;
    double r_ex = 0.05;

    double c = 0.6;
    double b = 1500;
    double k = 0.2;
    double h = 1000;
    double alpha = 0.5;
    double rho = 10;
    double e_0 = 2;

    // Parameters for DefineSimplifications
    double omega;
    double gamma;
    double q;
    double u;
    double pi_e;
    double sigma;

    public int shift;
    public void SetShift(int shift) {this.shift = shift; }

    [Header("UI TMP_Text")]
    public TMP_Text shiftText;
    public TMP_Text shiftText2;

    [Header("UI TMP_InputField")]
    public TMP_InputField textGasto;
    public TMP_InputField textTasaImp;
    public TMP_InputField textOma;
    public TMP_InputField textPIB;
    public TMP_InputField textInflacion;
    public TMP_InputField textTasaInt;
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
    double G;
    double t;
    double M;
    double OMA;
    double[] initialGuess;
    double[] finalGuess;
    
    void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;    
    }    

    public void SaveLocalData()
    {
        PlayerPrefs.SetInt("shift", shift);

        PlayerPrefs.SetString("G", textGasto.text);
        PlayerPrefs.SetString("t", textTasaImp.text );
        PlayerPrefs.SetString("OMA", textOma.text);
        PlayerPrefs.SetString("M", M.ToString());

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

    //en la ronda incial (turno 0), se asignan valores aleatorios en cada variable: G, t, M, initialGuess (y,r,p) -> (PIB, tasa interes, p)
    //se ejecuta la solucion y esos resultados aleatorios me generan un nuevo resultado para comenzar el turno 1: PIB, tasa interes, p, tasa inflacion y balance fiscal
    //los rangos de los valores varian de donde empiece el jugador
    // G = {50 - 1000} ++50
    // t = {0 - 1} ++0.1
    // M = {500 - 10000} ++500


    public void NewGame()
    {
        GameLoadManager.Instance.SetFinishedGame(false);

        shift = 1;
        initialGuess = new double[] { 819, 0.3, 2.5 }; //pdte determinar valores iniciales (y,r,p)

        calcularButton.gameObject.SetActive(true);
        siguienteTurnoButton.gameObject.SetActive(true);
        finalizarPartidaButton.gameObject.SetActive(false);
        
        System.Random random = new System.Random();        
        G = ( (random.Next(50, 101)) / 50 ) * 50;
        t = (random.Next(0, 11)) * 0.1;
        M = ( (random.Next(500, 10001)) / 500 ) * 500;

        DefineSimplifications(G, t, M);
        finalGuess = SolveEquations(initialGuess);
        initialGuess = finalGuess;
    }

    public void ContinueGame()
    {
        WindowGraph.Instance.GetLocalData();

        GameLoadManager.Instance.SetFinishedGame(PlayerPrefs.GetInt("finishedGame") == 1 ? true : false);
        M = Convert.ToDouble(PlayerPrefs.GetString("M"));//PREGUNTAR SI M LO COLOCA EL USUARIO EN LOS DEMAS TURNOS

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
            textGasto.text = PlayerPrefs.GetString("G");
            textTasaImp.text = PlayerPrefs.GetString("t");
            textOma.text = PlayerPrefs.GetString("OMA");
        }
    }   

    public void FinishedGame()
    {
        GameLoadManager.Instance.SetFinishedGame(true);

        NotificationsManager.Instance.QuestionNotifications("¿Quieres finalizar la partida?");
        NotificationsManager.Instance.SetYesButton(()=>
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

                G = Convert.ToDouble(textGasto.text);
                t = Convert.ToDouble(textTasaImp.text);
                OMA = (toggleCompra.isOn ? M + Convert.ToDouble(textOma.text) : M - Convert.ToDouble(textOma.text));
                
                DefineSimplifications(G, t, M);
                finalGuess = SolveEquations(initialGuess);
                GenerateReport();
                
                UI_System.Instance.SwitchScreens(resultadosScreen);
            });

        }
        else
            NotificationsManager.Instance.WarningNotifications("¡Llene todos los campos para calcular!");
    }

    void DefineSimplifications(double G, double t, double M)
    {
        omega = a + f + G + x1 * y_ex;
        gamma = 1 - c * (1 - t) + m1;
        q = 1 / alpha;
        u = (1 - alpha) / alpha;
        pi_e = 0;
        sigma = x2 - m2;
    }

    double[] SolveEquations(double[] initialGuess)
    {
        Func<double[], double[]> equations = vars =>
        {
            double y = vars[0];
            double r = vars[1];
            double p = vars[2];

            double da = gamma * y + sigma * rho * r - omega - sigma * e_0 - sigma * rho * r_ex;
            double m = k * y - h * r - M / p - h * pi_e;
            double oa = y - ((Math.Pow(A, q)) * ka * (Math.Pow(1 - alpha, u)) * (Math.Pow(p/w, u)));

            return new double[] { da, m, oa };
        };


        double accuracy = 1e-8; // Desired accuracy
        int maxIterations = 100; // Maximum number of iterations
        double jacobianStepSize = 1e-4; // Step size for numerical Jacobian approximation

        double[] solutionVector = Broyden.FindRoot(equations, initialGuess, accuracy, maxIterations, jacobianStepSize); //fsolve de python
        
        return solutionVector;
    }

    void GenerateReport()
    {
        double y = finalGuess[0];
        double r = finalGuess[1];
        double pFinal = finalGuess[2];

        double pInicial = initialGuess[2];

        double inf = ((pFinal - pInicial) / pInicial) * 100;
        double BF = (t * y) - G;

        textPIB.text = y.ToString("F2"); //ya no es PIB si no crecimiento economico y cambia la formula, PDTE
        textInflacion.text = inf.ToString("F2");
        textTasaInt.text = r.ToString("F2");
        textBalance.text = BF.ToString("F2");

        WindowGraph.Instance.shiftsList.Add(new Turnos(new Datos(G, t, OMA), new Resultados(y, inf, r, BF)));

        initialGuess = finalGuess;
    }
}