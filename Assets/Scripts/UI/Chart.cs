using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Chart : MonoBehaviour
{
    public RectTransform ChartPanel;
    public RectTransform LinePrefab;
    public RectTransform YearTick;

    void DrawChart(List<float> History)
    {
        int YearsAmount = 10;   //количество лет на графике
        Color redBrush = new Color(1, 0, 0);
        Color blueBrush = new Color(0, 0, 1);

        float xScale, yScale, yOffset;

        RectTransform ChartPanel = null;

        //Определяем начальный элемент истории
        int FirstInd = History.Count - YearsAmount;
        if (FirstInd < 0)
            FirstInd = 0;

        //Сначала удалим предыдущие графики
        while (ChartPanel.childCount > 0)
            DestroyImmediate(ChartPanel.GetChild(0).gameObject);

        xScale = ChartPanel.rect.width / (YearsAmount - 1);

        //Рисуем годы на графике
        int InitYear = 50 + FirstInd;   //51-й год - первый, где есть статистика

        for (int i = 0; i < YearsAmount; i++)
        {
            RectTransform Year = Instantiate(YearTick);
            Year.SetParent(ChartPanel);
            Year.localPosition = new Vector3(xScale * i, -3, 0);
            int tmpYear = InitYear + i;
            if (tmpYear >= 100)
                tmpYear -= 100;
            Year.transform.Find("Text").GetComponent<Text>().text = tmpYear.ToString("d2");
        }

        //Если в истории меньше двух значений, нечего рисовать
        if (History.Count < 2)
            return;


        float maxY = History.Max();
        float minY = History.Min();
        yScale = ChartPanel.rect.height / (maxY - minY);
        yOffset = minY;

        //значения горизонтальных линий
        ChartPanel.parent.Find("Value0").GetComponent<Text>().text = minY.ToString();
        ChartPanel.parent.Find("Value1").GetComponent<Text>().text = ((maxY + minY) / 2f).ToString();
        ChartPanel.parent.Find("Value2").GetComponent<Text>().text = maxY.ToString();

        //Вывод графиков
        Vector2 p1, p2;
        RectTransform Line;
        //Американский график
        for (int ind = 0; ind < History.Count - 1; ind++)
        {
            //Для рисования линии будем поворачивать и растягивать простой прямоугольник (Image с пустым спрайтом)
            //Начало линии будет в точке текущего значения статистики (х - год, у - значени), а конец в точке следующего значения из массива.
            p1.x = ind * xScale;
            p1.y = (Mathf.Min(History[ind], maxY) - yOffset) * yScale;

            Line = Instantiate(LinePrefab);
            Line.GetComponent<Image>().color = blueBrush;
            Line.SetParent(ChartPanel);
            Line.localPosition = p1;

            p2.x = (ind + 1) * xScale;
            p2.y = (Mathf.Min(History[ind + 1], maxY) - yOffset) * yScale;

            p1 = p2 - p1;
            Line.localScale = new Vector3(p1.magnitude, 1, 1);
            Line.rotation = Quaternion.FromToRotation(Vector3.right, p1);
        }

        //Советский график
        for (int ind = 0; ind < History.Count - 1; ind++)
        {
            p1.x = ind * xScale;
            p1.y = (Mathf.Min(History[ind], maxY) - yOffset) * yScale;

            Line = Instantiate(LinePrefab);
            Line.GetComponent<Image>().color = redBrush;
            Line.SetParent(ChartPanel);
            Line.localPosition = p1;

            p2.x = (ind + 1) * xScale;
            p2.y = (Mathf.Min(History[ind + 1], maxY) - yOffset) * yScale;

            p1 = p2 - p1;
            Line.localScale = new Vector3(p1.magnitude, 1, 1);
            Line.rotation = Quaternion.FromToRotation(Vector3.right, p1);
        }
    }
}
