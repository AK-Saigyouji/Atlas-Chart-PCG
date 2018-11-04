using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.AtlasGeneration;

namespace AKSaigyouji.AtlasGeneration
{
    public class MarkerMapperGO : MarkerMapper<RandomArrayGO>
    {
        protected override void InstantiateContent(RandomArrayGO spawner, Marker marker, Transform parent)
        {
            GameObject createdObject = Instantiate(spawner.RandomValue, parent);
            createdObject.transform.position = ComputePosition(marker.GlobalPositon);
            marker.Use();
        }
    }
}