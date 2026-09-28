using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindowTable : MonoBehaviour
{
    public GameObject[] infoTableList;

    public void EnabledTable()
    {
        List<Turnos> shiftsList = WindowGraph.Instance.shiftsList;

        for(int i = 0; i < shiftsList.Count; i ++)
        {
            Datos datos = shiftsList[i].datos;
            infoTableList[i].SetActive(true);

            Transform tabla = infoTableList[i].transform.Find("Tabla");
            SetCellText(tabla, "GASTO R", datos.G.ToString());
            SetCellText(tabla, "TASA IMPUESTO R", datos.t.ToString("F2"));
            SetCellText(tabla, "COMPRA VENTA R", datos.M.ToString());
        }
    }

    static void SetCellText(Transform tabla, string cellName, string text)
    {
        tabla.Find(cellName).GetComponent<TMP_Text>().text = text;
    }
}
