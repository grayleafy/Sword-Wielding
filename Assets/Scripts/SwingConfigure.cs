using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "configure/SwingConfigure")]
public class SwingConfigure : ScriptableObject
{
    [SerializeField] public float chargeTime;
    [SerializeField] public float attackTime; //攻击结束的时间
    [SerializeField] public AnimationCurve curve;
    [SerializeField] public ClipData[] chargeClipDatas;
    [SerializeField] public AnimationClip[] attackFromRightClips;
    [SerializeField] public AnimationClip[] attackFromUpClips;
    [SerializeField] public AnimationClip[] attackFromLeftClips;
    [SerializeField] public AnimationClip[] attackFromDownToRightClips;
    [SerializeField] public AnimationClip[] attackFromDownToLeftClips;
}
