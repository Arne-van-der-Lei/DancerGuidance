
using System;
using System.Diagnostics;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UISDFImageManager : UdonSharpBehaviour
{
    public float msTakenForUIUpdate = 0.5f;
    private DataList images = new DataList();
    private int currentIndex = 0;

    public void Register(UISDFImage image, RectTransform renderer)
    {
        DataList dataList = new DataList();
        dataList.Add(image);
        dataList.Add(renderer);
        images.Add(dataList);
    }
    
    public void Unregister(UISDFImage image)
    {
        for (int i = 0; i < images.Count; i++)
        {
            DataList dataList = images[i].DataList;
            if (dataList[0].Reference == image)
            {
                images.RemoveAt(i);
                break;
            }
        }
    }

    public void LateUpdate()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        int i = currentIndex;
        while (true)
        {
            if (i >= images.Count)
            {
                i = 0;
            }
            DataList dataList = images[i].DataList;
            RectTransform rectTransform = (RectTransform)dataList[1].Reference;
            if (rectTransform.hasChanged)
            {
                UISDFImage image = (UISDFImage)dataList[0].Reference;
                image.UpdateMesh();
                rectTransform.hasChanged = false;
            }
            i++;
            if (stopwatch.ElapsedTicks > msTakenForUIUpdate * 10000)
            {
                break;
            }
        }
        currentIndex = i;
        stopwatch.Stop();
    }
}
