using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpringDamperSystem;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace SpringAnimationTransition
{
    //参照坐标空间
    public enum CoordinateSpace
    {
        World, Local, Charactor
    }

    public enum FadeType
    {
        SpringDamper, Inertialization
    }

    public class SpringAnimatorController : MonoBehaviour
    {
        public Animator animator;
        int layerCount;
        int[] currentAnimationHash;
        bool isChanged = false;
        bool forceChange = false;

        [Header("参数设置")]
        [Range(0.01f, 2)]
        public float dampRatio = 1;
        [Range(0.01f, 0.5f)]
        public float halfLife = 0.2f;
        [SerializeField]
        FadeType fadeType = FadeType.Inertialization;
        [SerializeField]
        CoordinateSpace coordinateSpace = CoordinateSpace.Local;
        [SerializeField]
        bool updatePosition = true;
        [SerializeField]
        bool addRelativeVelocity = true;
        [SerializeField]
        bool autoCheck = false;
        //[SerializeField]
        //float maxPositionDiffs = 0.1f;
        //[SerializeField]
        //float maxAngerDiffs = Mathf.PI * 0.5f;

        [Header("骨骼")]
        [SerializeField]
        bool autoInitBoneDatas = true;
        [SerializeField]
        public List<BoneData> boneDatas;

        //一些中间变量,保留上一个动画的最后一帧的速度信息
        Vector3[] lastVelocitys;
        Vector3[] lastAngularVecitys;



        void Start()
        {
            animator = GetComponent<Animator>();
            layerCount = animator.layerCount;
            currentAnimationHash = new int[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                currentAnimationHash[i] = animator.GetCurrentAnimatorStateInfo(i).shortNameHash;
            }

            if (autoInitBoneDatas)
            {
                InitBoneData();
            }


            lastVelocitys = new Vector3[boneDatas.Count];
            lastAngularVecitys = new Vector3[boneDatas.Count];
        }

        //初始化人形骨骼数据
        void InitBoneData()
        {
            boneDatas = new();
            Transform temp;
            //身体
            temp = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.Spine);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            //左手
            temp = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            //右手
            temp = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            //左腿
            temp = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftToes);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            //右腿
            temp = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightToes);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));

            //头
            temp = animator.GetBoneTransform(HumanBodyBones.Head);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.Neck);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.Jaw);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));

            //左手手指
            temp = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));

            //右手手指
            temp = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightRingDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
            temp = animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
            if (temp != null) boneDatas.Add(new BoneData(temp, coordinateSpace));
        }

        //过渡骨骼动画
        void UpdateBoneDatas(float dt)
        {
            if (fadeType == FadeType.SpringDamper)
            {
                foreach (BoneData boneData in boneDatas)
                {
                    Vector3 posGoal = Vector3.zero;
                    Quaternion rotGoal = Quaternion.identity;
                    if (coordinateSpace == CoordinateSpace.World)
                    {
                        posGoal = boneData.transform.position;
                        rotGoal = boneData.transform.rotation;
                    }
                    else if (coordinateSpace == CoordinateSpace.Local)
                    {
                        posGoal = boneData.transform.localPosition;
                        rotGoal = boneData.transform.localRotation;
                    }

                    //位置过渡
                    if (updatePosition)
                    {
                        SpringDamper.SpringDamperExactVector(ref boneData.position, ref boneData.velocity, posGoal, Vector3.zero, dampRatio, halfLife, dt);

                    }
                    //旋转过渡
                    SpringDamper.SimpleSpringDamperExactQuat(ref boneData.rotation, ref boneData.angularVelocity, rotGoal, halfLife, dt);



                    if (coordinateSpace == CoordinateSpace.World)
                    {
                        boneData.transform.position = boneData.position;
                        boneData.transform.rotation = boneData.rotation;
                    }
                    else if (coordinateSpace == CoordinateSpace.Local)
                    {
                        boneData.transform.localPosition = boneData.position;
                        boneData.transform.localRotation = boneData.rotation;
                    }

                }
            }
            else if (fadeType == FadeType.Inertialization)
            {
                if (coordinateSpace == CoordinateSpace.Local)
                {
                    foreach (BoneData bone in boneDatas)
                    {
                        SpringDamper.SpringDamperExactVector(ref bone.offsetPosition, ref bone.offsetVelocity, Vector3.zero, Vector3.zero, dampRatio, halfLife, dt);
                        SpringDamper.SimpleSpringDamperExactQuat(ref bone.offsetRotation, ref bone.offsetAngularVelocity, Quaternion.identity, halfLife, dt);

                        bone.lastAnimatorPosition = bone.animatorPosition;
                        bone.lastAnimatorRotation = bone.animatorRotation;
                        bone.animatorPosition = bone.transform.localPosition;
                        bone.animatorRotation = bone.transform.localRotation;
                        bone.lastPosition = bone.position;
                        bone.lastRotation = bone.rotation;
                        bone.position = bone.animatorPosition + bone.offsetPosition;
                        bone.rotation = bone.offsetRotation * bone.animatorRotation;


                        bone.deltaTime = dt;

                        bone.transform.localPosition = bone.position;
                        bone.transform.localRotation = bone.rotation;
                    }
                }
            }
        }

        //检测是否切换了动画
        bool CheckChangeAnimation()
        {
            //float positionDiffs = 0;
            //float angerDiffs = 0;
            //foreach (BoneData bone in boneDatas)
            //{
            //    if (fadeType == FadeType.SpringDamper)
            //    {
            //        if (coordinateSpace == CoordinateSpace.Local)
            //        {
            //            positionDiffs += (bone.transform.localPosition - bone.position).magnitude;
            //            angerDiffs += Mathf.Abs(Mathf.Acos((bone.transform.localRotation * Quaternion.Inverse(bone.rotation)).normalized.w) * 2);
            //        }
            //    }
            //}

            //if (positionDiffs > maxPositionDiffs || angerDiffs > maxAngerDiffs) return true;
            //return false;

            if (forceChange)
            {
                forceChange = false;
                return true;
            }

            if (autoCheck)
            {
                bool flag = false;

                for (int i = 0; i < layerCount; i++)
                {
                    int stateHash = animator.GetCurrentAnimatorStateInfo(i).shortNameHash;
                    if (stateHash != currentAnimationHash[i])
                    {
                        flag = true;
                        currentAnimationHash[i] = stateHash;
                    }
                }
                return flag;
            }


            return false;
        }

        //附加相对速度
        void AddRelativeVelocity()
        {
            if (fadeType == FadeType.Inertialization)
            {
                if (coordinateSpace == CoordinateSpace.Local)
                {
                    int i = 0;
                    foreach (BoneData bone in boneDatas)
                    {
                        //if (i == 11)
                        //{
                        //    Debug.Log("start");
                        //}
                        //计算相对速度
                        bone.offsetVelocity = lastVelocitys[i] - (bone.transform.localPosition - bone.animatorPosition) / Time.deltaTime;
                        Vector3 angularVelocity = (bone.transform.localRotation * Quaternion.Inverse(bone.animatorRotation)).ToScaledAngleAxis() / Time.deltaTime;

                        float times = Mathf.Max(lastAngularVecitys[i].magnitude, angularVelocity.magnitude);
                        times = times * 2 / Mathf.PI + 5;

                        bone.offsetAngularVelocity = ((lastAngularVecitys[i] / times).ToQuaternion() * Quaternion.Inverse((angularVelocity / times).ToQuaternion())).ToScaledAngleAxis() * times;
                        //bone.offsetAngularVelocity = lastAngularVecitys[i] - angularVelocity;
                        //bone.offsetAngularVelocity = QuaternionHelper.EulerVelocityToAngular(QuaternionHelper.AngularVelocityToEuler(lastAngularVecitys[i]) - QuaternionHelper.AngularVelocityToEuler(angularVelocity));

                        //if (i == 11)
                        //{
                        //    Debug.Log("last " + lastAngularVecitys[i]);
                        //    Debug.Log("this" + angularVelocity);
                        //    Debug.Log("add " + bone.offsetAngularVelocity);
                        //    Debug.Log(lastAngularVecitys[i].ToQuaternion().ToScaledAngleAxis());
                        //    Debug.Log("\n\n");
                        //}

                        i++;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            //动画切换的第一帧结束后，更新前后动画的相对速度
            if (isChanged)
            {
                isChanged = false;
                if (addRelativeVelocity) AddRelativeVelocity();
            }

            if (CheckChangeAnimation())
            {
                isChanged = true;
                //重置偏移量
                if (fadeType == FadeType.Inertialization)
                {
                    if (coordinateSpace == CoordinateSpace.Local)
                    {
                        int i = 0;
                        foreach (BoneData bone in boneDatas)
                        {
                            //更新偏移距离
                            bone.offsetPosition = bone.position - bone.transform.localPosition;
                            bone.offsetRotation = bone.rotation * Quaternion.Inverse(bone.transform.localRotation);
                            bone.offsetRotation = bone.offsetRotation.Abs();

                            //更新相对于local坐标系的偏移速度
                            bone.offsetVelocity = (bone.position - bone.lastPosition) / bone.deltaTime;
                            bone.offsetAngularVelocity = (bone.rotation * Quaternion.Inverse(bone.lastRotation)).ToScaledAngleAxis() / bone.deltaTime;

                            //bone.offsetVelocity = Vector3.zero;
                            //bone.offsetAngularVelocity = Vector3.zero;

                            //保留最后一帧的速度信息
                            lastVelocitys[i] = bone.offsetVelocity;
                            lastAngularVecitys[i] = bone.offsetAngularVelocity;

                            i++;
                        }
                    }
                }
            }

            UpdateBoneDatas(Time.deltaTime);
        }

        public void ForceChange()
        {
            forceChange = true;
        }
    }
}

