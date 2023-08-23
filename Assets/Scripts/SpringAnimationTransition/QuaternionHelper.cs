using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpringAnimationTransition
{
    public static class QuaternionHelper
    {
        static Vector3 quat_log(Quaternion q, float eps = 1e-8f)
        {
            float length = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);

            if (length < eps)
            {
                return new Vector3(q.x, q.y, q.z);
            }
            else
            {
                float halfangle = Mathf.Acos(Mathf.Clamp(q.w, -1.0f, 1.0f));
                return halfangle * (new Vector3(q.x, q.y, q.z) / length);
            }
        }

        public static Vector3 ToScaledAngleAxis(this Quaternion q, float eps = 1e-8f)
        {
            return 2.0f * quat_log(q, eps);
        }

        static Quaternion quat_exp(Vector3 v, float eps = 1e-8f)
        {
            float halfangle = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);

            if (halfangle < eps)
            {
                return (new Quaternion(v.x, v.y, v.z, 1.0f)).normalized;
            }
            else
            {
                float c = Mathf.Cos(halfangle);
                float s = Mathf.Sin(halfangle) / halfangle;
                return new Quaternion(s * v.x, s * v.y, s * v.z, c);
            }
        }

        public static Quaternion ToQuaternion(this Vector3 v, float eps = 1e-8f)
        {
            return quat_exp(v / 2.0f, eps);
        }


        public static Vector3 AngularVelocityToEuler(Vector3 av)
        {
            float angle = av.magnitude;
            float t = angle * 2f / Mathf.PI + 5;

            av /= t;
            Vector3 res = av.ToQuaternion().eulerAngles * t;
            return res;
        }

        public static Vector3 EulerVelocityToAngular(Vector3 ev)
        {
            float t = Mathf.Max(Mathf.Abs(ev.x), Mathf.Abs(ev.y), Mathf.Abs(ev.z));
            t = t / 90 + 5;

            ev /= t;
            Vector3 res = Quaternion.Euler(ev).ToScaledAngleAxis() * t;
            return res;
        }

        public static Quaternion Abs(this Quaternion q)
        {
            Quaternion res = q;
            if (q.w < 0)
            {
                res.x = -q.x;
                res.y = -q.y;
                res.z = -q.z;
                res.w = -q.w;
            }
            return res;
        }
    }
}

