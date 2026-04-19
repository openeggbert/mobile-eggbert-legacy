// WindowsPhoneSpeedyBlupi, Version=1.0.0.5, Culture=neutral, PublicKeyToken=6db12cd62dbec439
// WindowsPhoneSpeedyBlupi.Misc
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using WindowsPhoneSpeedyBlupi;
using static WindowsPhoneSpeedyBlupi.Def;


namespace WindowsPhoneSpeedyBlupi
{
    public static class Misc
    {
        public static Rectangle RotateAdjust(Rectangle rect, double angle)
        {
            TinyPoint center = default(TinyPoint);
            center.X = rect.Width / 2;
            center.Y = rect.Height / 2;
            TinyPoint originalCenter = center;
            TinyPoint rotatedCenter = RotatePointRad(angle, originalCenter);
            int offsetX = rotatedCenter.X - originalCenter.X;
            int offsetY = rotatedCenter.Y - originalCenter.Y;
            return new Rectangle(
                rect.Left - offsetX, 
                rect.Top - offsetY, 
                rect.Width, 
                rect.Height
                );
        }

        public static TinyPoint RotatePointRad(double angle, TinyPoint p)
        {
            return RotatePointRad(default(TinyPoint), angle, p);
        }

        public static TinyPoint RotatePointRad(TinyPoint center, double angle, TinyPoint point)
        {
            TinyPoint relativePoint = default(TinyPoint);
            TinyPoint rotatedPoint = default(TinyPoint);
            relativePoint.X = point.X - center.X;
            relativePoint.Y = point.Y - center.Y;
            double sinAngle = Math.Sin(angle);
            double cosAngle = Math.Cos(angle);
            rotatedPoint.X = (int)((double)relativePoint.X * cosAngle - (double)relativePoint.Y * sinAngle);
            rotatedPoint.Y = (int)((double)relativePoint.X * sinAngle + (double)relativePoint.Y * cosAngle);
            rotatedPoint.X += center.X;
            rotatedPoint.Y += center.Y;
            return rotatedPoint;
        }

        public static double DegToRad(double angle)
        {
            return angle * Math.PI / 180.0;
        }

        public static int Approach(int actual, int final, int step)
        {
            if (actual < final)
            {
                actual = Math.Min(actual + step, final);
            }
            else if (actual > final)
            {
                actual = Math.Max(actual - step, final);
            }
            return actual;
        }

        public static int Speed(double speed, int max)
        {
            if (speed > 0.0)
            {
                return Math.Max((int)(speed * (double)max), 1);
            }
            if (speed < 0.0)
            {
                return Math.Min((int)(speed * (double)max), -1);
            }
            return 0;
        }

        public static TinyRect Inflate(TinyRect rect, int value)
        {
            TinyRect result = default(TinyRect);
            result.Left = rect.Left - value;
            result.Right = rect.Right + value;
            result.Top = rect.Top - value;
            result.Bottom = rect.Bottom + value;
            return result;
        }

        public static bool IsInside(TinyRect rect, TinyPoint p)
        {
            return p.X >= rect.Left && p.X <= rect.Right && p.Y >= rect.Top && p.Y <= rect.Bottom;
        }

        public static bool IntersectRect(out TinyRect dst, TinyRect src1, TinyRect src2)
        {
            dst = default(TinyRect);
            dst.Left = Math.Max(src1.Left, src2.Left);
            dst.Right = Math.Min(src1.Right, src2.Right);
            dst.Top = Math.Max(src1.Top, src2.Top);
            dst.Bottom = Math.Min(src1.Bottom, src2.Bottom);
            return !IsRectEmpty(dst);
        }

        public static bool UnionRect(out TinyRect dst, TinyRect src1, TinyRect src2)
        {
            dst = default(TinyRect);
            dst.Left = Math.Min(src1.Left, src2.Left);
            dst.Right = Math.Max(src1.Right, src2.Right);
            dst.Top = Math.Min(src1.Top, src2.Top);
            dst.Bottom = Math.Max(src1.Bottom, src2.Bottom);
            return !IsRectEmpty(dst);
        }

        private static bool IsRectEmpty(TinyRect rect)
        {
            if (rect.Left < rect.Right)
            {
                return rect.Top >= rect.Bottom;
            }
            return true;
        }

    }
}