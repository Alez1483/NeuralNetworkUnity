using UnityEngine;
using System.Runtime.CompilerServices;
using static System.Math;

public struct Matrix3x3
{
    /*
    m00 m10 m20
    m01 m11 m21
    m02 m12 m22
    */

    public double m00;
    public double m10;
    public double m20;
    public double m01;
    public double m11;
    public double m21;
    public double m02;
    public double m12;
    public double m22;

    public double this[int i]
    {
        get
        {
            switch (i)
            {
                case 0: return m00;
                case 1: return m10;
                case 2: return m20;
                case 3: return m01;
                case 4: return m11;
                case 5: return m21;
                case 6: return m02;
                case 7: return m12;
                case 8: return m22;
                default: throw new System.IndexOutOfRangeException($"Index of {i} is outside of the range of matrix");
            }
        }
        set
        {
            switch (i)
            {
                case 0: m00 = value; break; 
                case 1: m10 = value; break;
                case 2: m20 = value; break;
                case 3: m01 = value; break;
                case 4: m11 = value; break;
                case 5: m21 = value; break;
                case 6: m02 = value; break;
                case 7: m12 = value; break;
                case 8: m22 = value; break;
                default: throw new System.IndexOutOfRangeException($"Index of {i} is outside of the range of matrix");
            }
        }
    }

    public double this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return this[x + 3 * y];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            this[x + 3 * y] = value;
        }
    }

    public static Matrix3x3 operator*(Matrix3x3 a, Matrix3x3 b)
    {
        Matrix3x3 output;

        output.m00 = a.m00 * b.m00 + a.m10 * b.m01 + a.m20 * b.m02;
        output.m10 = a.m00 * b.m10 + a.m10 * b.m11 + a.m20 * b.m12;
        output.m20 = a.m00 * b.m20 + a.m10 * b.m21 + a.m20 * b.m22;

        output.m01 = a.m01 * b.m00 + a.m11 * b.m01 + a.m21 * b.m02;
        output.m11 = a.m01 * b.m10 + a.m11 * b.m11 + a.m21 * b.m12;
        output.m21 = a.m01 * b.m20 + a.m11 * b.m21 + a.m21 * b.m22;

        output.m02 = a.m02 * b.m00 + a.m12 * b.m01 + a.m22 * b.m02;
        output.m12 = a.m02 * b.m10 + a.m12 * b.m11 + a.m22 * b.m12;
        output.m22 = a.m02 * b.m20 + a.m12 * b.m21 + a.m22 * b.m22;

        return output;
    }

    public static Vector3 operator*(Matrix3x3 m, Vector3 v)
    {
        Vector3 output;

        output.x = (float)(m.m00 * v.x + m.m10 * v.y + m.m20 * v.z);
        output.y = (float)(m.m01 * v.x + m.m11 * v.y + m.m21 * v.z);
        output.z = (float)(m.m02 * v.x + m.m12 * v.y + m.m22 * v.z);

        return output;
    }

    public static Matrix3x3 TranslationMatrix(Vector2 translation)
    {
        Matrix3x3 output;

        output.m00 = 1;
        output.m10 = 0;
        output.m20 = translation.x;

        output.m01 = 0;
        output.m11 = 1;
        output.m21 = translation.y;

        output.m02 = 0;
        output.m12 = 0;
        output.m22 = 1;

        return output;
    }

    public static Matrix3x3 RotationMatrix(double angle)
    {
        angle *= PI / 180.0;

        double sin = Sin(angle);
        double cos = Cos(angle);

        Matrix3x3 output;

        output.m00 = cos;
        output.m10 = -sin;
        output.m20 = 0;

        output.m01 = sin;
        output.m11 = cos;
        output.m21 = 0;

        output.m02 = 0;
        output.m12 = 0;
        output.m22 = 1;

        return output;
    }

    public static Matrix3x3 ScaleMatrix(Vector2 scale)
    {
        Matrix3x3 output;

        output.m00 = scale.x;
        output.m10 = 0;
        output.m20 = 0;

        output.m01 = 0;
        output.m11 = scale.y;
        output.m21 = 0;

        output.m02 = 0;
        output.m12 = 0;
        output.m22 = 1;

        return output;
    }
}
