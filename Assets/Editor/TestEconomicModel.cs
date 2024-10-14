using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MathNet.Numerics.RootFinding;

public class TestEconomicModel
{
    private EconomicModel economicModel;

    [SetUp]
    public void Setup()
    {   
        GameObject gameObject = new GameObject();
        economicModel = gameObject.AddComponent<EconomicModel>();

        economicModel.SetShift(1);

        economicModel.initialGuess = new double[] { 800, 0.05, 1.1 };
        economicModel.finalGuess = new double[] { 900, 0.06, 1.2 };

        economicModel.G = 150;
        economicModel.t = 0.1;
        economicModel.M = 5000;

        economicModel.a = 160;
        economicModel.f = 100;
        economicModel.w = 50;
        economicModel.ka = 30000;
        economicModel.L = 225;
        economicModel.A = 1;
        economicModel.x1 = 0.2;
        economicModel.x2 = 30;
        economicModel.m1 = 0.06;
        economicModel.m2 = 10;
        economicModel.y_ex = 20000;
        economicModel.r_ex = 0.05;
        economicModel.c = 0.6;
        economicModel.b = 1500;
        economicModel.k = 0.2;
        economicModel.h = 1000;
        economicModel.alpha = 0.5;
        economicModel.rho = 10;
        economicModel.e_0 = 2;
    }

    [Test]
    public void TestDefineSimplifications()
    {
        double G = 150;
        double t = 0.1;
        double M = 5000;

        economicModel.DefineSimplifications(G, t, M);

        Assert.AreEqual(160 + 100 + G + 0.2 * 20000, economicModel.omega);
        Assert.AreEqual(1 - 0.6 * (1 - t) + 0.06, economicModel.gamma);
        Assert.AreEqual(1 / 0.5, economicModel.q);
        Assert.AreEqual((1 - 0.5) / 0.5, economicModel.u);
        Assert.AreEqual(0, economicModel.pi_e);
        Assert.AreEqual(30 - 10, economicModel.sigma);
    }

    [Test]
    public void TestSolveEquations()
    {
        double G = 150;
        double t = 0.1;
        double M = 5000;

        double[] initialGuess = new double[] { 800, 0.05, 1.1 };        
        double[] expectedSolution = new double[] { 8030.99, 1.41, 26.76};

        economicModel.DefineSimplifications(G, t, M);
        double[] solution = economicModel.SolveEquations(initialGuess);

        Assert.AreEqual(expectedSolution[0], solution[0], 0.01); // PIB (y)
        Assert.AreEqual(expectedSolution[1], solution[1], 0.01); // Tasa de interés (r)
        Assert.AreEqual(expectedSolution[2], solution[2], 0.01); // Precio (p)
    }
    
    [Test]
    public void TestCalculateReport()
    {
        economicModel.CalculateReport();

        Assert.AreEqual(12.50f, Math.Round(economicModel.variacionPIB, 2)); // variación PIB
        Assert.AreEqual(9.09f, Math.Round(economicModel.inf), 2); // inflación
        Assert.AreEqual(0.06f, Math.Round(economicModel.r), 2); // tasa de interés
        Assert.AreEqual(-60.0f, Math.Round(economicModel.BF), 2); // balance fiscal
    }
}
