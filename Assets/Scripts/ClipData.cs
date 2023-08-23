using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClipData
{
    [SerializeField] public AnimationClip clip;
    [SerializeField] public Vector2 dir;
    public float angle;
}
