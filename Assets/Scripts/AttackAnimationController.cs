using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UIElements;


[RequireComponent(typeof(Animator))]
public class AttackAnimationController : MonoBehaviour
{
    InputHandler inputHandler;
    SpringAnimationTransition.SpringAnimatorController springAnimatorController;
    PlayableGraph m_graph;
    AnimationLayerMixerPlayable m_layer;
    ScriptPlayable<SwingPlayable> swingPlayable;



    [SerializeField] SwingConfigure swingConfigure;
    [SerializeField] GameObject rightHand;

    [SerializeField] RuntimeAnimatorController animatorController;
    //[SerializeField] AnimatorController animatorController;
    [SerializeField] AvatarMask mask;
    [SerializeField] AnimationCurve curve;

    [SerializeField] GameObject virtualMouse;
    [SerializeField] GameObject controlMouse;
    [SerializeField] float followHalfLife = 0.2f;
    [SerializeField] public float radius = 50;
    [SerializeField] Vector2 virtualMousePos = Vector2.zero;
    [SerializeField] float mouseSpeed = 0.1f;
    [SerializeField] float attackDistance = 70f;



    private void Start()
    {
        inputHandler = GetComponent<InputHandler>();
        springAnimatorController = GetComponent<SpringAnimationTransition.SpringAnimatorController>();
        virtualMousePos = new Vector2(Screen.width / 2, Screen.height / 2);

        InitPlayable();
    }

    private void Update()
    {
        RestrictMouse();
        FollowVirtualMouse(Time.deltaTime);


        //if (!inputHandler.lockMouse)
        //{
        //    if (swingPlayable.GetBehaviour().playState == SwingPlayable.PlayState.Charge && CheckAttack())
        //    {
        //        inputHandler.lockMouse = true;
        //        //Debug.Log("\n\nattack");
        //        swingPlayable.GetBehaviour().ChargeToAttack(virtualMouse.GetComponent<RectTransform>().position - controlMouse.GetComponent<RectTransform>().position);
        //        //Debug.Log("virtual: " + virtualMouse.GetComponent<RectTransform>().position);
        //        //Debug.Log("control: " + controlMouse.GetComponent<RectTransform>().position);
        //    }

        //}
        if (swingPlayable.GetBehaviour().playState == SwingPlayable.PlayState.Charge && CheckAttack())
        {
            swingPlayable.GetBehaviour().ChargeToAttack(virtualMouse.GetComponent<RectTransform>().position - controlMouse.GetComponent<RectTransform>().position);
        }


        //更新playable的参数
        swingPlayable.GetBehaviour().UpdateControl(controlMouse.GetComponent<RectTransform>().localPosition.x / radius, controlMouse.GetComponent<RectTransform>().localPosition.y / radius);
    }

    //更新虚拟鼠标位置并限制鼠标范围
    void RestrictMouse()
    {
        Vector2 center = Vector2.zero;
        Vector2 mouseDelta = inputHandler.mouseDelta;
        virtualMousePos += mouseDelta * mouseSpeed;
        Vector2 dir = virtualMousePos - center;

        //角度约束
        Vector2 rightDown = new Vector2(1, -1).normalized;
        Vector2 down = new Vector2(0, -1).normalized;
        if (dir.x > 0 && (dir.x * rightDown.y - dir.y * rightDown.x) * (dir.x * down.y - dir.y * down.x) < 0)
        {
            if (Vector2.Dot(dir, rightDown) > Vector2.Dot(dir, down))
            {
                //dir = rightDown * Vector2.Dot(dir, rightDown);
            }
            else
            {
                //dir = down * Vector2.Dot(dir, down);
            }
        }


        //半径约束
        if (dir.magnitude > radius)
        {
            dir = dir.normalized * radius;
            //Mouse.current.WarpCursorPosition(mousePos);
        }

        virtualMousePos = center + dir;



        //更新UI
        if (virtualMouse)
        {
            virtualMouse.GetComponent<RectTransform>().localPosition = virtualMousePos;
        }
    }

    //控制鼠标跟随虚拟鼠标
    void FollowVirtualMouse(float delta)
    {
        Vector3 dir = controlMouse.GetComponent<RectTransform>().position - virtualMouse.GetComponent<RectTransform>().position;
        if (dir == Vector3.zero) return;
        float len = Common.SpringDamper.SpringDamper.damper_exact(dir.magnitude, 0, followHalfLife, delta);
        controlMouse.GetComponent<RectTransform>().position = virtualMouse.GetComponent<RectTransform>().position + dir.normalized * len;
    }

    //检测是否攻击
    bool CheckAttack()
    {

        Vector2 dir = virtualMouse.GetComponent<RectTransform>().position - controlMouse.GetComponent<RectTransform>().position;
        return dir.magnitude > attackDistance;
    }

    //playable初始化
    void InitPlayable()
    {
        m_graph = PlayableGraph.Create("sword");
        var output = AnimationPlayableOutput.Create(m_graph, "AttackOutput", GetComponent<Animator>());


        //最上层，混合动画控制器和挥舞动作
        m_layer = AnimationLayerMixerPlayable.Create(m_graph, 2, true);
        output.SetSourcePlayable(m_layer);


        //动画控制器
        var ctrlPlayable = AnimatorControllerPlayable.Create(m_graph, animatorController);
        m_graph.Connect(ctrlPlayable, 0, m_layer, 0);
        m_layer.SetInputWeight(0, 1);


        //别的
        swingPlayable = ScriptPlayable<SwingPlayable>.Create(m_graph);
        swingPlayable.GetBehaviour().Initialize(m_graph, swingPlayable, swingConfigure, gameObject, rightHand, virtualMouse, controlMouse, springAnimatorController);
        m_graph.Connect(swingPlayable, 0, m_layer, 1);
        m_layer.SetLayerMaskFromAvatarMask(1, mask);
        m_layer.SetInputWeight(1, 1);


        m_graph.Play();



    }


    private void OnDisable()
    {
        m_graph.Destroy();
    }
}

//动画必须设置为非循环
public class SwingPlayable : PlayableBehaviour
{
    public enum PlayState
    {
        Charge, Attack
    }
    public PlayState playState;
    float attackLast = 0;
    GameObject charactor;
    GameObject rightHand;
    GameObject virtualMouse;
    GameObject controlMouse;
    SpringAnimationTransition.SpringAnimatorController springAnimatorController;
    Vector3 handPos;
    AnimationCurve curve;

    PlayableGraph m_graph;
    Playable root;
    //蓄力相关playable
    Playable chargeMixer;
    //攻击相关playable
    Playable attackMixer;
    Playable horizontalMixer;
    Playable verticalMixer;
    Playable leftMixer;
    Playable rightMixer;
    Playable upMixer;
    Playable downMixer;
    Playable downToRight;
    Playable downToLeft;

    float attackDuration = 0.1f;
    float chargeTime;
    float attackTime; //攻击结束的时间
    //操作杆的坐标
    Vector2 controlPos = Vector2.zero;
    float[] angles;

    public void UpdateControl(float x, float y)
    {
        controlPos.x = x;
        controlPos.y = y;
    }

    public void Initialize(PlayableGraph graph, Playable owner, SwingConfigure swingConfigure, GameObject charactor, GameObject rightHand, GameObject virtualMouse, GameObject controlMouse, SpringAnimationTransition.SpringAnimatorController springAnimatorController)
    {
        //保存数据
        m_graph = graph;
        this.rightHand = rightHand;
        this.charactor = charactor;
        chargeTime = swingConfigure.chargeTime;
        attackTime = swingConfigure.attackTime;
        this.virtualMouse = virtualMouse;
        this.controlMouse = controlMouse;
        this.springAnimatorController = springAnimatorController;
        attackDuration = swingConfigure.attackTime - swingConfigure.chargeTime;
        this.curve = swingConfigure.curve;

        //默认蓄力
        playState = PlayState.Charge;

        //绑定自己
        root = AnimationMixerPlayable.Create(graph, 2);
        owner.SetInputCount(1);
        graph.Connect(root, 0, owner, 0);
        owner.SetInputWeight(0, 1);

        //绑定蓄力和攻击的混合树
        chargeMixer = AnimationMixerPlayable.Create(graph, swingConfigure.chargeClipDatas.Length);
        graph.Connect(chargeMixer, 0, root, 0);
        root.SetInputWeight(0, 1);
        attackMixer = AnimationMixerPlayable.Create(graph, 2);
        graph.Connect(attackMixer, 0, root, 1);
        root.SetInputWeight(1, 0);

        //蓄力混合树初始化
        angles = new float[swingConfigure.chargeClipDatas.Length];
        //转换为[0-2PI)的角度
        for (int i = 0; i < angles.Length; i++)
        {
            swingConfigure.chargeClipDatas[i].dir.Normalize();
            angles[i] = Mathf.Asin(swingConfigure.chargeClipDatas[i].dir.y);
            if (swingConfigure.chargeClipDatas[i].dir.x < 0)
            {
                angles[i] = Mathf.PI - angles[i];
            }
            if (angles[i] < 0) angles[i] += 2 * Mathf.PI;
            swingConfigure.chargeClipDatas[i].angle = angles[i];
        }
        //按照角度排序
        Array.Sort(swingConfigure.chargeClipDatas, (ClipData a, ClipData b) => a.angle.CompareTo(b.angle));
        Array.Sort(angles, (float a, float b) => a.CompareTo(b));
        //生成clip节点
        for (int i = 0; i < swingConfigure.chargeClipDatas.Length; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(graph, swingConfigure.chargeClipDatas[i].clip);
            clipPlayable.SetApplyFootIK(false);
            clipPlayable.Pause();
            graph.Connect(clipPlayable, 0, chargeMixer, i);
        }

        //攻击混合树
        //水平和竖直层
        horizontalMixer = AnimationMixerPlayable.Create(graph, 2);
        verticalMixer = AnimationMixerPlayable.Create(graph, 2);
        graph.Connect(horizontalMixer, 0, attackMixer, 0);
        graph.Connect(verticalMixer, 0, attackMixer, 1);
        //相反方向
        leftMixer = AnimationMixerPlayable.Create(graph, swingConfigure.attackFromLeftClips.Length);
        rightMixer = AnimationMixerPlayable.Create(graph, swingConfigure.attackFromRightClips.Length);
        upMixer = AnimationMixerPlayable.Create(graph, swingConfigure.attackFromUpClips.Length);
        downMixer = AnimationMixerPlayable.Create(graph, 2);
        graph.Connect(leftMixer, 0, horizontalMixer, 0);
        graph.Connect(rightMixer, 0, horizontalMixer, 1);
        graph.Connect(upMixer, 0, verticalMixer, 0);
        graph.Connect(downMixer, 0, verticalMixer, 1);
        //动画剪辑
        for (int i = 0; i < swingConfigure.attackFromLeftClips.Length; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(graph, swingConfigure.attackFromLeftClips[i]);
            graph.Connect(clipPlayable, 0, leftMixer, i);
        }
        for (int i = 0; i < swingConfigure.attackFromRightClips.Length; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(graph, swingConfigure.attackFromRightClips[i]);
            graph.Connect(clipPlayable, 0, rightMixer, i);
        }
        for (int i = 0; i < swingConfigure.attackFromUpClips.Length; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(graph, swingConfigure.attackFromUpClips[i]);
            graph.Connect(clipPlayable, 0, upMixer, i);
        }
        //下方向特殊处理
        downToRight = AnimationMixerPlayable.Create(graph, swingConfigure.attackFromDownToRightClips.Length);
        downToLeft = AnimationMixerPlayable.Create(graph, swingConfigure.attackFromDownToLeftClips.Length);
        graph.Connect(downToRight, 0, downMixer, 0);
        graph.Connect(downToLeft, 0, downMixer, 1);
        for (int i = 0; i < swingConfigure.attackFromDownToRightClips.Length; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(graph, swingConfigure.attackFromDownToRightClips[i]);
            graph.Connect(clipPlayable, 0, downToRight, i);
        }
        for (int i = 0; i < swingConfigure.attackFromDownToLeftClips.Length; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(graph, swingConfigure.attackFromDownToLeftClips[i]);
            graph.Connect(clipPlayable, 0, downToLeft, i);
        }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);






        if (playState == PlayState.Attack)
        {
            attackLast += Time.deltaTime;
            if (SetTimeAndSpeed(attackMixer, attackTime) == true)
            {
                AttackToCharge();
            }
        }

        if (true || playState == PlayState.Charge)
        {
            //根据长度计算动画时间
            //float t = Mathf.Clamp01(controlPos.magnitude) * ((AnimationClipPlayable)chargeMixer.GetInput(0)).GetAnimationClip().length;
            float t = Mathf.Clamp01(controlPos.magnitude) * chargeTime;


            //根据方向来选择权重
            float controlAngle = 0;
            if (controlPos.magnitude > 0)
            {
                controlAngle = Mathf.Asin(controlPos.y / controlPos.magnitude);
                if (controlPos.x < 0)
                {
                    controlAngle = Mathf.PI - controlAngle;
                }
                if (controlAngle < 0) controlAngle += Mathf.PI * 2;
                //Debug.Log("controlangle = " + controlAngle);
            }
            for (int i = 0; i < angles.Length; i++)
            {
                chargeMixer.GetInput(i).SetTime(t);
                if (i == angles.Length - 1 && controlAngle >= angles[i])
                {
                    float alpha = controlAngle - angles[i];
                    float beta = angles[0] + 2 * Mathf.PI - controlAngle;
                    chargeMixer.SetInputWeight(i, beta / (alpha + beta));
                    chargeMixer.SetInputWeight(0, alpha / (alpha + beta));
                }
                else if (controlAngle >= angles[i] && controlAngle < angles[i + 1])
                {
                    float alpha = controlAngle - angles[i];
                    float beta = angles[i + 1] - controlAngle;
                    chargeMixer.SetInputWeight(i, beta / (alpha + beta));
                    chargeMixer.SetInputWeight(i + 1, alpha / (alpha + beta));
                    i++;
                    if (i < angles.Length) chargeMixer.GetInput(i).SetTime(t);
                }
                else
                {
                    chargeMixer.SetInputWeight(i, 0);
                }
            }
        }

    }

    //遍历并设置时间小于等于t, 返回是否全部到达t
    bool SetTimeAndSpeed(Playable root, float t)
    {
        bool res = true;
        if (root.GetPlayableType() == typeof(AnimationClipPlayable))
        {
            if (root.GetTime() < t)
            {
                res = false;
            }
            else
            {
                root.SetTime(t);
            }
            //root.SetSpeed(curve.Evaluate(attackLast / (attackDuration)));
        }
        int cnt = root.GetInputCount();
        for (int i = 0; i < cnt; i++)
        {
            res = res && SetTimeAndSpeed(root.GetInput(i), t);
        }
        return res;
    }

    public void AttackToCharge()
    {
        Debug.Log("charge");
        playState = PlayState.Charge;
        //切换蓄力和攻击混合树的权重
        root.SetInputWeight(0, 1);
        root.SetInputWeight(1, 0);

        controlMouse.GetComponent<RectTransform>().position = virtualMouse.GetComponent<RectTransform>().position;
        //更新controlpos
        controlPos = controlMouse.GetComponent<RectTransform>().localPosition / 50;

        springAnimatorController.halfLife = 0.2f;
        springAnimatorController.ForceChange();
    }

    //切换状态
    public void ChargeToAttack(Vector2 attackDirection)
    {
        Debug.Log("attack");
        //切换状态
        playState = PlayState.Attack;
        attackLast = 0;

        //Debug.Log("dir = " + attackDirection);

        //保存右手的位置
        handPos = rightHand.transform.position;
        handPos = charactor.transform.InverseTransformPoint(handPos);
        //Debug.Log("handpos = " + handPos);

        //切换蓄力和攻击混合树的权重
        root.SetInputWeight(0, 0);
        root.SetInputWeight(1, 1);

        //水平和竖直的权重
        float angle = Mathf.Asin(Mathf.Abs(attackDirection.y) / attackDirection.magnitude);
        attackMixer.SetInputWeight(0, (Mathf.PI / 2 - angle) * 2 / Mathf.PI);
        attackMixer.SetInputWeight(1, angle * 2 / Mathf.PI);

        //水平的起点计算, 先时间， 避免动作的起伏误差,                                       竖直方向计算时根据水平位置为0时选择从左边出发
        //先计算起点时间
        if (attackDirection.x >= 0)
        {
            horizontalMixer.SetInputWeight(0, 1);
            horizontalMixer.SetInputWeight(1, 0);
            SetFromTime(leftMixer, handPos, 0);
            //根据竖直的差距计算混合权重
            SetWeightByLinearMix(leftMixer, handPos, 1);
        }
        else
        {
            horizontalMixer.SetInputWeight(0, 0);
            horizontalMixer.SetInputWeight(1, 1);
            SetFromTime(rightMixer, handPos, 0);
            //根据竖直的差距计算混合权重
            SetWeightByLinearMix(rightMixer, handPos, 1);
        }

        //输出时间和权重
        //Debug.Log("left");
        for (int i = 0; i < leftMixer.GetInputCount(); i++)
        {
            //Debug.Log("t = " + leftMixer.GetInput(i).GetTime() + "\nweight = " + leftMixer.GetInputWeight(i));
            //PrintPos(leftMixer.GetInput(i));
        }

        //竖直方向
        if (attackDirection.y < 0)
        {
            verticalMixer.SetInputWeight(0, 1);
            verticalMixer.SetInputWeight(1, 0);
            SetFromTime(upMixer, handPos, 1);
            //根据水平的差距计算混合权重
            SetWeightByLinearMix(upMixer, handPos, 0);
        }
        else
        {
            verticalMixer.SetInputWeight(0, 0);
            verticalMixer.SetInputWeight(1, 1);
            //如果向右挥剑
            if (attackDirection.x >= 0)
            {
                downMixer.SetInputWeight(0, 1);
                downMixer.SetInputWeight(1, 0);
                SetFromTime(downToRight, handPos, 1);
                //根据水平的差距计算混合权重
                SetWeightByLinearMix(downToRight, handPos, 0);
            }
            else
            {
                downMixer.SetInputWeight(0, 0);
                downMixer.SetInputWeight(1, 1);
                SetFromTime(downToLeft, handPos, 1);
                //根据水平的差距计算混合权重
                SetWeightByLinearMix(downToLeft, handPos, 0);
            }

        }

        //输出时间和权重
        //Debug.Log("up");
        for (int i = 0; i < upMixer.GetInputCount(); i++)
        {
            //Debug.Log("t = " + upMixer.GetInput(i).GetTime() + "\nweight = " + upMixer.GetInputWeight(i));
            //PrintPos(upMixer.GetInput(i));
        }

        springAnimatorController.halfLife = 0.05f;
        springAnimatorController.ForceChange();
    }

    //线性混合权重
    void SetWeightByLinearMix(Playable mixer, Vector3 target, int axis)
    {
        if (mixer.GetInputCount() <= 0) return;
        //根据竖直的差距计算混合权重
        SetZero(mixer);
        float lastDis = DistanceToTarget(mixer.GetInput(0), (float)mixer.GetInput(0).GetTime(), target, axis);
        for (int i = 1; i < mixer.GetInputCount(); i++)
        {
            float dis = DistanceToTarget(mixer.GetInput(i), (float)mixer.GetInput(i).GetTime(), target, axis);
            if (lastDis * dis <= 0)
            {
                mixer.SetInputWeight(i - 1, Mathf.Abs(dis / (lastDis - dis)));
                mixer.SetInputWeight(i, Mathf.Abs(lastDis / (lastDis - dis)));
                break;
            }
            lastDis = dis;

            if (i == mixer.GetInputCount() - 1)
            {
                if (Mathf.Abs(DistanceToTarget(mixer.GetInput(0), (float)mixer.GetInput(0).GetTime(), target, axis)) <= Mathf.Abs(dis))
                {
                    mixer.SetInputWeight(0, 1);
                }
                else
                {
                    mixer.SetInputWeight(i, 1);
                }
            }
        }
    }

    //计算起点最接近目标点的动画剪辑时间并设置, target为局部坐标系下的目标
    void SetFromTime(Playable mixer, Vector3 target, int axis)
    {
        for (int i = 0; i < mixer.GetInputCount(); i++)
        {
            //二分
            float lt = chargeTime;
            float rt = attackTime;
            float ld = DistanceToTarget(mixer.GetInput(i), lt, target, axis);
            float rd = DistanceToTarget(mixer.GetInput(i), rt, target, axis);
            //如果零点不在范围内
            if (ld * rd >= 0)
            {
                if (MathF.Abs(ld) < MathF.Abs(rd))
                {
                    mixer.GetInput(i).SetTime(lt);
                }
                else
                {
                    mixer.GetInput(i).SetTime(rt);
                }
                mixer.GetInput(i).SetSpeed((attackTime - mixer.GetInput(i).GetTime()) / attackDuration);
                continue;
            }

            float mt;
            int iteration = 10;
            while (iteration-- > 0)
            {
                mt = lt + (rt - lt) / 2;
                float md = DistanceToTarget(mixer.GetInput(i), mt, target, axis);
                if (md * ld < 0)
                {
                    rt = mt;
                    rd = md;
                }
                else
                {
                    lt = mt;
                    ld = md;
                }
            }

            mixer.GetInput(i).SetTime(lt);
            mixer.GetInput(i).SetSpeed((attackTime - mixer.GetInput(i).GetTime()) / attackDuration);
        }
    }

    float DistanceToTarget(Playable clipPlayable, float t, Vector3 target, int axis)
    {
        AnimationClip clip = ((AnimationClipPlayable)clipPlayable).GetAnimationClip();
        clip.SampleAnimation(charactor, t);

        Vector3 pos = rightHand.transform.position;
        pos = charactor.transform.InverseTransformPoint(pos);

        float dis = pos[axis] - target[axis];
        return dis;
    }

    void PrintPos(Playable clipPlayable)
    {
        AnimationClip clip = ((AnimationClipPlayable)clipPlayable).GetAnimationClip();
        clip.SampleAnimation(charactor, (float)clipPlayable.GetTime());

        Vector3 pos = rightHand.transform.position;
        pos = charactor.transform.InverseTransformPoint(pos);

        Debug.Log("pos = " + pos);
    }

    //将节点的所有输入权重置为零
    void SetZero(Playable mixer)
    {
        for (int i = 0; i < mixer.GetInputCount(); i++)
        {
            mixer.SetInputWeight(i, 0);
        }
    }
}