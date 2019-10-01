using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleDiagram : MonoBehaviour
{
    [SerializeField] List<float> shares;
    [SerializeField] List<Color> colors;
    //[SerializeField] GameObject imgGO;
    [SerializeField] Sprite Sprite;

    List<Image> imageList;

    private void Awake()
    {
        if (shares.Count != colors.Count)
        {
            throw new System.Exception("Incorrect component properties.");
        }

        //Скрываем маркер диаграммы.
        GetComponent<Image>().enabled = false;

        imageList = new List<Image>();

        GameObject newGO;
        Image newImage;
        RectTransform newRectTransform;
        for (int i = 0; i < colors.Count; i++)
        {
            newGO = new GameObject("sh" + i);
            newGO.transform.SetParent(transform, false);

            newImage = (Image)newGO.AddComponent(typeof(Image));
            newImage.sprite = Sprite;
            newImage.raycastTarget = false;
            newImage.type = Image.Type.Filled;
            newImage.fillMethod = Image.FillMethod.Radial360;
            newImage.fillClockwise = true;
            newImage.fillOrigin = 2;
            newImage.fillCenter = true;

            newRectTransform = newGO.GetComponent<RectTransform>();
            newRectTransform.anchorMin = Vector2.zero;
            newRectTransform.anchorMax = Vector2.one;
            newRectTransform.sizeDelta = Vector2.one;

            imageList.Add(newImage);
        }

        Refresh();
    }

    void Refresh()
    {
        Image img;
        float shrSum = 0f;
        for (int i = 0; i < imageList.Count; i++)
        {
            img = imageList[i];
            img.color = colors[i];
            img.fillAmount = shares[i];

            img.transform.Rotate(new Vector3(0, 0, -360f * shrSum));
            shrSum += shares[i];
        }
    }
}
