using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class WindowTable : MonoBehaviour
{
    public GameObject[] infoTableList;

    public void EnabledTable()
    {
        for(int i = 0; i < WindowGraph.Instance.shiftsList.Count; i ++)
        {
            infoTableList[i].SetActive(true);
            infoTableList[i].transform.Find("Tabla").transform.Find("GASTO R").GetComponent<TMP_Text>().text = WindowGraph.Instance.shiftsList[i].datos.G.ToString("F2");
            infoTableList[i].transform.Find("Tabla").transform.Find("TASA IMPUESTO R").GetComponent<TMP_Text>().text = WindowGraph.Instance.shiftsList[i].datos.t.ToString("F2");
            infoTableList[i].transform.Find("Tabla").transform.Find("COMPRA VENTA R").GetComponent<TMP_Text>().text = WindowGraph.Instance.shiftsList[i].datos.OMA.ToString("F2");
        }
    }
}
