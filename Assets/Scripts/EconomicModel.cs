using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EconomicModel : Singleton<EconomicModel>
{
    bool finishedGame => GameLoadManager.Instance.GetFinishedGame();

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
    public static double L = 225; // trabajo //// == Pandemia mata el 25% se la población. (No se usa en ninguna ecuación)
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
    public static double gamma, q, u;
    // Término constante de la oferta agregada: A^q * ka * (1 - alpha)^u
    static double supplyFactor;

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
    public TMP_Text textCurrentGasto;
    public TMP_Text textCurrentTasaImp;
    public TMP_Text textCurrentOma;

    [Header("UI Toggle")]
    public Toggle toggleRestaGasto;
    public Toggle toggleSumaGasto;
    public Toggle toggleRestaTasaImp;
    public Toggle toggleSumaTasaImp;
    public Toggle toggleVenta;
    public Toggle toggleCompra;


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
    const char GUESS_SEPARATOR = ';';
    const char LEGACY_GUESS_SEPARATOR = '-'; // partidas guardadas antes del cambio a ';'

    [HideInInspector]
    public double variacionPIB, inf, saldo;
    [HideInInspector]
    public double[] initialGuess, finalGuess;

    TMP_InputField[] inputFields;
    Toggle[] toggles;

    protected override void Awake()
    {
        base.Awake();
        inputFields = new[] { textGasto, textTasaImp, textOma };
        toggles = new[] { toggleSumaGasto, toggleRestaGasto, toggleSumaTasaImp, toggleRestaTasaImp, toggleCompra, toggleVenta };
    }

    void Start()
    {
        inputValidations = InputValidations.Instance;
    }

    #region Persistencia

    public void SaveLocalData()
    {
        PlayerPrefs.SetInt("shift", shift);

        PlayerPrefs.SetString("G", textGasto.text); // gasto
        PlayerPrefs.SetString("t", textTasaImp.text); // tasa impositiva
        PlayerPrefs.SetString("M", textOma.text); // bonos

        PlayerPrefs.SetString("initialGuess", string.Join(GUESS_SEPARATOR,
            initialGuess.Select(v => v.ToString(CultureInfo.InvariantCulture))));
    }

    void LoadInitialGuess()
    {
        string savedGuess = PlayerPrefs.GetString("initialGuess");
        char separator = savedGuess.Contains(GUESS_SEPARATOR) ? GUESS_SEPARATOR : LEGACY_GUESS_SEPARATOR;

        initialGuess = savedGuess
            .Split(separator)
            .Select(v => double.Parse(v.Replace(",", "."), CultureInfo.InvariantCulture))
            .ToArray();
    }

    #endregion

    #region Flujo de partida

    //en la ronda incial (turno 0), se asignan valores aleatorios en cada variable: G, t, M, initialGuess (y,r,p) -> (PIB, tasa interes, precio)
    //se ejecuta la solucion y esos resultados aleatorios me generan un nuevo resultado para comenzar el turno 1: PIB, tasa interes, p, tasa inflacion y balance fiscal
    //los rangos de los valores varian de donde empiece el jugador
    // G = {50 - 500} ++50
    // t = {0 - 0.3} ++0.01
    // M = {0 - 40} ++5

    private (double G, double t, double M) GenerateRandomPolicy()
    {
        // el rango del Random es hasta 3 para que varíe pequeño, y de 1 a 5 para que no tome el 0 de Oferta monetaria
        double Grand = RandomStep(inputValidations.limInfGasto, inputValidations.limSupGasto, inputValidations.stepGasto, 0, 2);
        double trand = RandomStep(inputValidations.limInfTasaImp, inputValidations.limSupTasaImp, inputValidations.stepTasaImp, 5, 11);
        double Mrand = RandomStep(inputValidations.limInfOma, inputValidations.limSupOma, inputValidations.stepOma, 2, 3);

        // pdte en la parte aleatoria tenemos variacion pib real muy alta

        return (Grand, trand, Mrand);
    }

    // Valor aleatorio = limInf + n * step, con n en [minSteps, maxSteps), acotado a [limInf, limSup]
    static double RandomStep(double limInf, double limSup, double step, int minSteps, int maxSteps)
    {
        return Math.Clamp(limInf + UnityEngine.Random.Range(minSteps, maxSteps) * step, limInf, limSup);
    }

    public void NewGame()
    {
        shift = 1;
        SetGameButtons(true);

        // Aleatorio por primera vez
        (G, t, M) = GenerateRandomPolicy();
        initialGuess = new double[] { 510, 0.3, 2.5 };
        RunModel();

        // Aleatorio por segunda vez
        (G, t, M) = GenerateRandomPolicy();
        RunModel();

        GenerateReport();
        NotifyNewValues();
    }

    public void ContinueGame()
    {
        LoadInitialGuess();

        bool inProgress = shift <= GameLoadManager.SHIFTS;
        SetGameButtons(inProgress);
        if (!inProgress)
            finalizarPartidaButton.interactable = !finishedGame;

        if(shift > 1)
        {
            textCurrentGasto.text = FormatCurrentValue(PlayerPrefs.GetString("G")); // gasto
            textCurrentTasaImp.text = FormatCurrentValue(PlayerPrefs.GetString("t")); // tasa impositiva
            textCurrentOma.text = FormatCurrentValue(PlayerPrefs.GetString("M")); // bonos
        }
    }

    public void FinishedGame()
    {
        GameLoadManager.Instance.SetFinishedGame(true);

        NotificationsManager.Instance.QuestionNotifications("¿Quieres finalizar la partida?");
        NotificationsManager.Instance.SetYesButton(() =>
        {
            ClearInputs();
            finalizarPartidaButton.interactable = false;
        });
    }

    public void Calculate()
    {
        if (!AreInputsComplete())
        {
            NotificationsManager.Instance.WarningNotifications("¡Llene todos los campos para calcular!");
            return;
        }

        NotificationsManager.Instance.QuestionNotifications("¿Quieres calcular?");
        NotificationsManager.Instance.SetYesButton(()=> {
            if(shift == GameLoadManager.SHIFTS)
                SetGameButtons(false);

            G = ApplyChange(G, textGasto.text, toggleSumaGasto, toggleRestaGasto); // gasto
            t = ApplyChange(t, textTasaImp.text, toggleSumaTasaImp, toggleRestaTasaImp); // tasa impositiva
            M = ApplyChange(M, textOma.text, toggleCompra, toggleVenta); // bonos

            RunModel();
            GenerateReport();

            UI_System.Instance.SwitchScreens(resultadosScreen);
            shift+=1;

            ClearInputs();
        });
    }

    // Aplica una carta de evento sobre un instrumento de política y recalcula el modelo
    public void ApplyEvent(OpcionesResultados opcion, double cambio)
    {
        switch (opcion)
        {
            case OpcionesResultados.Gasto_Publico:
                G += cambio;
                break;
            case OpcionesResultados.Tasa_Impositiva:
                t += cambio;
                break;
            case OpcionesResultados.Bonos:
                M += cambio;
                break;
        }

        RunModel();
        ShowCurrentResults();
        ShowCurrentPolicy();
    }

    #endregion

    #region UI

    public void EnabledInputData()
    {
        foreach (TMP_InputField field in inputFields)
            field.enabled = true;
        foreach (Toggle toggle in toggles)
            toggle.interactable = true;
    }

    void ClearInputs()
    {
        foreach (TMP_InputField field in inputFields)
            field.text = "";
        foreach (Toggle toggle in toggles)
            toggle.isOn = false;
    }

    bool AreInputsComplete()
    {
        return inputFields.All(field => !string.IsNullOrEmpty(field.text))
            && (toggleSumaGasto.isOn || toggleRestaGasto.isOn)
            && (toggleSumaTasaImp.isOn || toggleRestaTasaImp.isOn)
            && (toggleCompra.isOn || toggleVenta.isOn);
    }

    // true: partida en curso (calcular / siguiente turno); false: solo finalizar partida
    void SetGameButtons(bool inProgress)
    {
        calcularButton.gameObject.SetActive(inProgress);
        siguienteTurnoButton.gameObject.SetActive(inProgress);
        finalizarPartidaButton.gameObject.SetActive(!inProgress);
    }

    static double ApplyChange(double current, string input, Toggle suma, Toggle resta)
    {
        return suma.isOn ? current + Convert.ToDouble(input)
             : resta.isOn ? current - Convert.ToDouble(input)
             : current;
    }

    void ShowCurrentResults()
    {
        WindowGraph.Instance.currentPIB.text = FormatPercent(variacionPIB); // variacion PIB
        WindowGraph.Instance.currentTInf.text = FormatPercent(inf); // tasa inflacion
        WindowGraph.Instance.currentSaldo.text = saldo.ToString("F2"); // balance fiscal
    }

    void ShowCurrentPolicy()
    {
        textCurrentGasto.text = FormatCurrentValue(G);
        textCurrentTasaImp.text = FormatCurrentValue(t);
        textCurrentOma.text = FormatCurrentValue(M);
    }

    public void NotifyNewValues()
    {
        NotificationsManager.Instance.WarningNotifications(
            "Los nuevos valores son:\n\nVariación PIB: " + FormatPercent(variacionPIB) +
            "\n<b>Tasa inflación: </b>" + FormatPercent(inf) +
            "\n<b>Balance fiscal: </b>" + saldo.ToString("F2")
        );
    }

    static string FormatPercent(double value) => value.ToString("F2") + "%";
    static string FormatCurrentValue(double value) => "<b>Valor actual =</b> " + value.ToString("F2");
    static string FormatCurrentValue(string value) => "<b>Valor actual =</b> " + value;

    #endregion

    #region Modelo económico

    // Resuelve el modelo con la política actual (G, t, M) y actualiza los indicadores
    void RunModel()
    {
        DefineSimplifications(t);
        finalGuess = SolveEquations();
        CalculateReport();
    }

    public void DefineSimplifications(double t)
    {
        gamma = (1 - c * (1 - t));

        q = 1 / alpha;
        u = (1 - alpha) / alpha;
        supplyFactor = Math.Pow(A, q) * ka * Math.Pow(1 - alpha, u);
    }

    static void Doxn(double[] vars, double[] f)
    {
        double y = vars[0];
        double r = Math.Max(vars[1], EPS);
        double p = Math.Max(vars[2], EPS);

        f[0] = gamma * y - C - I - G + b * r;                // demanda agregada
        f[1] = M / p - k * y + h * r;                        // mercado de dinero
        f[2] = y - supplyFactor * Math.Pow(p / w, u);        // oferta agregada
    }

    public double[] SolveEquations()
    {
        double[] x = (double[])initialGuess.Clone();

        bool ok = MinpackLikeSolver.Solve(
            Doxn,
            x,
            maxIterations: 200,
            tol: 1e-12,
            lambda0: 1e-3,
            stepRel: 1e-7,
            out _
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

        // Se colocan límites a los valores resultantes: variacion PIB, tasa inflacion y balance fiscal
        // variacion PIB {-3 y 5}
        // tasa inflacion {1 y 15}
        // balance fiscal {sin límites}

        variacionPIB = Math.Clamp(PercentChange(yInicial, yFinal), -3, 5);
        inf = Math.Clamp(PercentChange(pInicial, pFinal), 1, 15); // Inflación (\pi) = [(p_1 -p_0)/p_0 ]* 100

        // ingreso del gobierno Nominal
        double ingreso = t * yFinal;
        saldo = ingreso - G; // balance fiscal

        initialGuess = finalGuess;
    }

    static double PercentChange(double inicial, double final) => ((final - inicial) / inicial) * 100;

    public void GenerateReport()
    {
        textPIB.text = FormatPercent(variacionPIB); // variacion PIB
        textInflacion.text = FormatPercent(inf); // tasa inflacion
        textBalance.text = saldo.ToString("F2"); // balance fiscal

        ShowCurrentResults();

        WindowGraph.Instance.shiftsList.Add(new Turnos(new Datos(G, t, M), new Resultados(variacionPIB, inf, saldo)));
    }

    #endregion
}
