using System;
using UnityEngine;

using static System.Math;

namespace BGC.Mathematics
{
    public static class BezierCurves
    {
        public static void PlotCubicSplineClosedCurve(
            int[] x, int[] y,
            Texture2D contourTex,
            Color contourColor)
        {
            PlotCubicSplineClosedCurve(x, y,
                addPoint: (x0, y0) => SetPixel(x0, y0, contourTex, contourColor));
        }

        public static void PlotCubicSpline(int[] x, int[] y,
            Texture2D contourTex,
            Color contourColor,
            bool skipEnds = true)
        {
            PlotCubicSpline(x, y,
                addPoint: (x0, y0) => SetPixel(x0, y0, contourTex, contourColor),
                skipEnds: skipEnds);
        }


        public static void PlotCubicSplineClosedCurve(
            int[] x, int[] y,
            Action<int, int> addPoint)
        {
            int[] newX = new int[x.Length + 5];
            int[] newY = new int[y.Length + 5];

            for (int i = 0; i < newX.Length; i++)
            {
                newX[i] = x[i % x.Length];
                newY[i] = y[i % x.Length];
            }

            PlotCubicSpline(newX, newY,
                addPoint: addPoint,
                skipEnds: true);
        }

        /// <summary>
        ///From http://members.chello.at/easyfilter/bresenham.pdf
        /// </summary>
        public static void PlotCubicSpline(int[] x, int[] y,
            Action<int, int> addPoint,
            bool skipEnds = true)
        {
            int n = x.Length - 1;
            /* plot cubic spline, destroys input arrays x,y */
            const int M_MAX = 6;

            /* diagonal constants of matrix */
            float mi = 0.25f;
            float[] m = new float[M_MAX];
            int x3 = x[n - 1];
            int y3 = y[n - 1];
            int x4 = x[n];
            int y4 = y[n];
            int i;
            int x0;
            int y0;
            int x1;
            int y1;
            int x2;
            int y2;

            /* need at least 4 points P[0]..P[n] */
            Debug.Assert(n > 2);

            /* first row of matrix */
            x0 = 12 * x[1] - 3 * x[0];
            y0 = 12 * y[1] - 3 * y[0];
            x[1] = x0;
            y[1] = y0;
            for (i = 2; i < n; i++)
            {
                /* foreward sweep */
                if (i - 2 < M_MAX)
                {
                    mi = 0.25f / (2f - mi);
                    m[i - 2] = mi;
                }

                x0 = (int)(12 * x[i] - 2 * x0 * mi + 0.5);
                y0 = (int)(12 * y[i] - 2 * y0 * mi + 0.5);
                x[i] = x0;
                y[i] = y0;
            }

            /* correct last row */
            x2 = (int)((x0 - 3 * x4) / (7 - 4 * mi) + 0.5);
            y2 = (int)((y0 - 3 * y4) / (7 - 4 * mi) + 0.5);

            if (!skipEnds)
            {
                //The Last piece
                PlotCubicBezier(
                    x0: x3,
                    y0: y3,
                    x1: (x2 + x4) / 2,
                    y1: (y2 + y4) / 2,
                    x2: x4,
                    y2: y4,
                    x3: x4,
                    y3: y4,
                    addPoint: addPoint);
            }

            if (n - 3 < M_MAX)
            {
                mi = m[n - 3];
            }

            x1 = (int)((x[n - 2] - 2 * x2) * mi + 0.5);
            y1 = (int)((y[n - 2] - 2 * y2) * mi + 0.5);

            for (i = n - 3; i > 0; i--)
            {
                /* back substitution */
                if (i <= M_MAX)
                {
                    mi = m[i - 1];
                }

                x0 = (int)((x[i] - 2 * x1) * mi + 0.5);
                y0 = (int)((y[i] - 2 * y1) * mi + 0.5);

                /* reconstruct P[i] */
                x4 = (int)((x0 + 4 * x1 + x2 + 3) / 6.0);
                y4 = (int)((y0 + 4 * y1 + y2 + 3) / 6.0);

                //Potentially skip the second-to-last piece
                if (i != n - 3 || !skipEnds)
                {
                    PlotCubicBezier(
                        x0: x4,
                        y0: y4,
                        x1: (int)((2 * x1 + x2) / 3 + 0.5),
                        y1: (int)((2 * y1 + y2) / 3 + 0.5),
                        x2: (int)((x1 + 2 * x2) / 3 + 0.5),
                        y2: (int)((y1 + 2 * y2) / 3 + 0.5),
                        x3: x3,
                        y3: y3,
                        addPoint: addPoint);
                }

                x3 = x4;
                y3 = y4;
                x2 = x1;
                y2 = y1;
                x1 = x0;
                y1 = y0;
            }

            x0 = x[0];
            y0 = y[0];

            /* reconstruct P[1] */
            x4 = (int)((3 * x0 + 7 * x1 + 2 * x2 + 6) / 12.0);
            y4 = (int)((3 * y0 + 7 * y1 + 2 * y2 + 6) / 12.0);

            if (!skipEnds)
            {
                //The second piece
                PlotCubicBezier(
                    x0: x4,
                    y0: y4,
                    x1: (int)((2 * x1 + x2) / 3 + 0.5),
                    y1: (int)((2 * y1 + y2) / 3 + 0.5),
                    x2: (int)((x1 + 2 * x2) / 3 + 0.5),
                    y2: (int)((y1 + 2 * y2) / 3 + 0.5),
                    x3: x3,
                    y3: y3,
                    addPoint: addPoint);

                //The first piece
                PlotCubicBezier(
                    x0: x0,
                    y0: y0,
                    x1: x0,
                    y1: y0,
                    x2: (x0 + x1) / 2,
                    y2: (y0 + y1) / 2,
                    x3: x4,
                    y3: y4,
                    addPoint: addPoint);
            }
        }



        /// <summary>
        ///From http://members.chello.at/easyfilter/bresenham.pdf
        /// </summary>
        public static void PlotCubicBezier(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3,
            Action<int, int> addPoint)
        {
            /* plot any cubic Bezier curve */
            int n = 0;
            int i = 0;
            long xc = x0 + x1 - x2 - x3;
            long xa = xc - 4 * (x1 - x2);
            long xb = x0 - x1 - x2 + x3;
            long xd = xb + 4 * (x1 + x2);
            long yc = y0 + y1 - y2 - y3;
            long ya = yc - 4 * (y1 - y2);
            long yb = y0 - y1 - y2 + y3;
            long yd = yb + 4 * (y1 + y2);
            float fx0 = x0;
            float fx1;
            float fx2;
            float fx3;
            float fy0 = y0;
            float fy1;
            float fy2;
            float fy3;
            double t1 = xb * xb - xa * xc;
            double t2;
            double[] t = new double[5];

            /* sub-divide curve at gradient sign changes */
            if (xa == 0)
            {
                /* horizontal */
                if (Abs(xc) < 2 * Abs(xb))
                {
                    /* one change */
                    t[n++] = xc / (2.0 * xb);
                }
            }
            else if (t1 > 0.0)
            {
                /* two changes */
                t2 = Sqrt(t1);
                t1 = (xb - t2) / xa;

                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }

                t1 = (xb + t2) / xa;

                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }
            }

            t1 = yb * yb - ya * yc;

            if (ya == 0)
            {
                /* vertical */
                if (Abs(yc) < 2 * Abs(yb))
                {
                    /* one change */
                    t[n++] = yc / (2.0 * yb);
                }
            }
            else if (t1 > 0.0)
            {
                /* two changes */
                t2 = Sqrt(t1);
                t1 = (yb - t2) / ya;
                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }

                t1 = (yb + t2) / ya;
                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }
            }

            /* bubble sort of 4 points */
            for (i = 1; i < n; i++)
            {
                if ((t1 = t[i - 1]) > t[i])
                {
                    t[i - 1] = t[i];
                    t[i] = t1;
                    i = 0;
                }
            }

            /* begin / end point */
            t1 = -1.0;
            t[n] = 1.0;
            for (i = 0; i <= n; i++)
            {
                /* plot each segment separately */
                /* sub-divide at t[i-1], t[i] */
                t2 = t[i];
                fx1 = (float)((t1 * (t1 * xb - 2.0 * xc) - t2 * (t1 * (t1 * xa - 2.0 * xb) + xc) + xd) / 8.0 - fx0);
                fy1 = (float)((t1 * (t1 * yb - 2.0 * yc) - t2 * (t1 * (t1 * ya - 2.0 * yb) + yc) + yd) / 8.0 - fy0);
                fx2 = (float)((t2 * (t2 * xb - 2.0 * xc) - t1 * (t2 * (t2 * xa - 2.0 * xb) + xc) + xd) / 8.0 - fx0);
                fy2 = (float)((t2 * (t2 * yb - 2.0 * yc) - t1 * (t2 * (t2 * ya - 2.0 * yb) + yc) + yd) / 8.0 - fy0);
                fx3 = (float)((t2 * (t2 * (3.0 * xb - t2 * xa) - 3.0 * xc) + xd) / 8.0);
                fx0 -= fx3;
                fy3 = (float)((t2 * (t2 * (3.0 * yb - t2 * ya) - 3.0 * yc) + yd) / 8.0);
                fy0 -= fy3;

                /* scale bounds to int */
                x3 = (int)Floor(fx3 + 0.5f);
                y3 = (int)Floor(fy3 + 0.5f);

                if (fx0 != 0.0)
                {
                    fx0 = (x0 - x3) / fx0;
                    fx1 *= fx0;
                    fx2 *= fx0;
                }

                if (fy0 != 0.0)
                {
                    fy0 = (y0 - y3) / fy0;
                    fy1 *= fy0;
                    fy2 *= fy0;
                }

                /* segment t1 - t2 */
                if (x0 != x3 || y0 != y3)
                {
                    PlotCubicBezierSeg(
                        x0: x0,
                        y0: y0,
                        x1: x0 + fx1,
                        y1: y0 + fy1,
                        x2: x0 + fx2,
                        y2: y0 + fy2,
                        x3: x3,
                        y3: y3,
                        addPoint: addPoint);
                }

                x0 = x3;
                y0 = y3;
                fx0 = fx3;
                fy0 = fy3;
                t1 = t2;
            }
        }

        /// <summary>
        ///From http://members.chello.at/easyfilter/bresenham.pdf
        /// </summary>
        public static void PlotCubicBezierSeg(int x0, int y0, float x1, float y1, float x2, float y2, int x3, int y3,
            Action<int, int> addPoint)
        {
            /* plot limited cubic Bezier segment */
            int f;
            int fx;
            int fy;
            int leg = 1;

            /* step direction */
            int sx = x0 < x3 ? 1 : -1;
            int sy = y0 < y3 ? 1 : -1;
            float xc = -Abs(x0 + x1 - x2 - x3);
            float xa = xc - 4 * sx * (x1 - x2);
            float xb = sx * (x0 - x1 - x2 + x3);
            float yc = -Abs(y0 + y1 - y2 - y3);
            float ya = yc - 4 * sy * (y1 - y2);
            float yb = sy * (y0 - y1 - y2 + y3);

            double ab;
            double ac;
            double bc;
            double cb;
            double xx;
            double xy;
            double yy;
            double dx;
            double dy;
            double ex;
            double EP = 0.01;

            /* check for curve restrains */
            /* slope P0-P1 == P2-P3 and (P0-P3 == P1-P2 or no slope change) */
            Debug.Assert((x1 - x0) * (x2 - x3) < EP && ((x3 - x0) * (x1 - x2) < EP || xb * xb < xa * xc + EP));
            Debug.Assert((y1 - y0) * (y2 - y3) < EP && ((y3 - y0) * (y1 - y2) < EP || yb * yb < ya * yc + EP));
            if (xa == 0 && ya == 0)
            {
                /* quadratic Bezier */
                /* new midpoint */
                sx = (int)Floor((3 * x1 - x0 + 1) / 2);
                sy = (int)Floor((3 * y1 - y0 + 1) / 2);

                PlotQuadBezierSeg(
                    x0: x0,
                    y0: y0,
                    x1: sx,
                    y1: sy,
                    x2: x3,
                    y2: y3,
                    addPoint: addPoint);

                return;
            }

            /* line lengths */
            x1 = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0) + 1;
            x2 = (x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3) + 1;
            do
            {
                /* loop over both ends */
                ab = xa * yb - xb * ya;
                ac = xa * yc - xc * ya;
                bc = xb * yc - xc * yb;

                /* P0 part of self-intersection loop? */
                ex = ab * (ab + ac - 3 * bc) + ac * ac;

                /* calculate resolution */
                f = ex > 0 ? 1 : (int)Sqrt(1 + 1024 / x1);

                /* increase resolution */
                ab *= f;
                ac *= f;
                bc *= f;
                ex *= f * f;

                /* init differences of 1st degree */
                xy = 9 * (ab + ac + bc) / 8;
                cb = 8 * (xa - ya);
                dx = 27 * (8 * ab * (yb * yb - ya * yc) + ex * (ya + 2 * yb + yc)) / 64 - ya * ya * (xy - ya);
                dy = 27 * (8 * ab * (xb * xb - xa * xc) - ex * (xa + 2 * xb + xc)) / 64 - xa * xa * (xy + xa);
                /* init differences of 2nd degree */
                xx = 3 * (3 * ab * (3 * yb * yb - ya * ya - 2 * ya * yc) - ya * (3 * ac * (ya + yb) + ya * cb)) / 4;
                yy = 3 * (3 * ab * (3 * xb * xb - xa * xa - 2 * xa * xc) - xa * (3 * ac * (xa + xb) + xa * cb)) / 4;
                xy = xa * ya * (6 * ab + 6 * ac - 3 * bc + cb);
                ac = ya * ya;
                cb = xa * xa;
                xy = 3 * (xy + 9 * f * (cb * yb * yc - xb * xc * ac) - 18 * xb * yb * ab) / 8;

                if (ex < 0)
                {
                    /* negate values if inside self-intersection loop */
                    dx = -dx;
                    dy = -dy;
                    xx = -xx;
                    yy = -yy;
                    xy = -xy;
                    ac = -ac;
                    cb = -cb;
                }

                /* init differences of 3rd degree */
                ab = 6 * ya * ac;
                ac = -6 * xa * ac;
                bc = 6 * ya * cb;
                cb = -6 * xa * cb;

                /* error of 1st step */
                dx += xy;
                ex = dx + dy;
                dy += xy;

                bool boundaryHit = false;
                double pxy = xy;

                for (fx = fy = f; x0 != x3 && y0 != y3;)
                {
                    /* plot curve */
                    addPoint.Invoke(x0, y0);

                    do
                    {
                        /* move sub-steps of one pixel */
                        if (dx > pxy || dy < pxy)
                        {
                            /* confusing values */
                            goto exit;
                        }

                        /* save value for test of y step */
                        y1 = (float)(2 * ex - dy);
                        if (2 * ex >= dx)
                        {
                            /* x sub-step */
                            fx--;

                            dx += xx;
                            ex += dx;

                            xy += ac;
                            dy += xy;

                            if (!boundaryHit)
                            {
                                pxy = xy;
                            }

                            yy += bc;
                            xx += ab;
                        }

                        if (y1 <= 0)
                        {
                            /* y sub-step */
                            fy--;

                            dy += yy;
                            ex += dy;

                            xy += bc;
                            dx += xy;

                            if (!boundaryHit)
                            {
                                pxy = xy;
                            }

                            xx += ac;
                            yy += cb;
                        }
                    }
                    while (fx > 0 && fy > 0); /* pixel complete? */

                    /* x step */
                    if (2 * fx <= f)
                    {
                        x0 += sx;
                        fx += f;
                    }

                    /* y step */
                    if (2 * fy <= f)
                    {
                        y0 += sy;
                        fy += f;
                    }

                    if (!boundaryHit && dx < 0 && dy > 0)
                    {
                        boundaryHit = true;
                        /* pixel ahead valid */
                        pxy = EP;
                    }
                }

                exit:

                /* swap legs */
                int tempInt = x0;
                x0 = x3;
                x3 = tempInt;
                sx = -sx;
                xb = -xb;


                tempInt = y0;
                y0 = y3;
                y3 = tempInt;
                sy = -sy;
                yb = -yb;
                x1 = x2;

            }
            while (leg-- > 0); /* try other end */

            /* remaining part in case of cusp or crunode */
            PlotLine(
                x0: x0,
                y0: y0,
                x1: x3,
                y1: y3,
                addPoint: addPoint);
        }

        /// <summary>
        ///From http://members.chello.at/easyfilter/bresenham.pdf
        /// </summary>
        public static void PlotQuadBezierSeg(int x0, int y0, int x1, int y1, int x2, int y2,
            Action<int, int> addPoint)
        {
            /* plot a limited quadratic Bezier segment */
            int sx = x2 - x1;
            int sy = y2 - y1;

            /* relative values for checks */
            long xx = x0 - x1;
            long yy = y0 - y1;
            long xy;

            /* curvature */
            double dx;
            double dy;
            double err;
            double cur = xx * sy - yy * sx;

            /* sign of gradient must not change */
            Debug.Assert(xx * sx <= 0 && yy * sy <= 0);

            if (sx * (long)sx + sy * (long)sy > xx * xx + yy * yy)
            {
                /* begin with longer part */
                /* swap P0 P2 */
                x2 = x0;
                x0 = sx + x1;
                y2 = y0;
                y0 = sy + y1;
                cur = -cur;
            }

            if (cur != 0)
            {
                /* no straight line */
                /* x step direction */
                xx += sx;
                sx = x0 < x2 ? 1 : -1;
                xx *= sx;

                /* y step direction */
                yy += sy;
                sy = y0 < y2 ? 1 : -1;
                yy *= sy;

                /* differences 2nd degree */
                xy = 2 * xx * yy;
                xx *= xx;
                yy *= yy;

                if (cur * sx * sy < 0)
                {
                    /* negated curvature? */
                    xx = -xx;
                    yy = -yy;
                    xy = -xy;
                    cur = -cur;
                }

                /* differences 1st degree */
                dx = 4.0 * sy * cur * (x1 - x0) + xx - xy;
                dy = 4.0 * sx * cur * (y0 - y1) + yy - xy;

                /* error 1st step */
                xx += xx;
                yy += yy;
                err = dx + dy + xy;
                do
                {
                    /* plot curve */
                    addPoint.Invoke(x0, y0);

                    if (x0 == x2 && y0 == y2)
                    {
                        /* last pixel -> curve finished */
                        return;
                    }

                    /* save value for test of y step */
                    bool yTrigger = 2 * err < dx;

                    if (2 * err > dy)
                    {
                        /* x step */
                        x0 += sx;
                        dx -= xy;
                        dy += yy;
                        err += dy;
                    }

                    if (yTrigger)
                    {
                        /* y step */
                        y0 += sy;
                        dy -= xy;
                        dx += xx;
                        err += dx;
                    }

                }
                while (dy < dx); /* gradient negates -> algorithm fails */
            }

            /* plot remaining part to end */
            PlotLine(
                x0: x0,
                y0: y0,
                x1: x2,
                y1: y2,
                addPoint: addPoint);
        }

        /// <summary>
        ///From http://members.chello.at/easyfilter/bresenham.pdf
        /// </summary>
        public static void PlotLine(int x0, int y0, int x1, int y1,
            Action<int, int> addPoint)
        {
            int dx = Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            /* error value e_xy */
            int e2;

            while (true)
            {
                addPoint.Invoke(x0, y0);

                e2 = 2 * err;

                if (e2 >= dy)
                {
                    /* e_xy+e_x > 0 */
                    if (x0 == x1)
                    {
                        break;
                    }

                    err += dy; x0 += sx;
                }

                if (e2 <= dx)
                {
                    /* e_xy+e_y < 0 */
                    if (y0 == y1)
                    {
                        break;
                    }

                    err += dx; y0 += sy;
                }
            }
        }

        /// <summary>
        /// Use a curried version of this method for the AddPoint action above.
        /// Invoke like (x,y) => SetPixel(x, y, localContourTex, localColor, 2); 
        /// </summary>
        public static void SetPixel(
            int x,
            int y,
            Texture2D contourTex,
            Color contourColor,
            int rad = 2)
        {
            int rad_sq = rad * rad;

            for (int dx = -rad; dx <= rad; dx++)
            {
                for (int dy = -rad; dy <= rad; dy++)
                {
                    if (dx * dx + dy * dy <= rad_sq)
                    {
                        if (x + dx >= 0 && x + dx < contourTex.width &&
                            y + dy >= 0 && y + dy < contourTex.height)
                        {
                            contourTex.SetPixel(x + dx, y + dy, contourColor);
                        }
                    }
                }
            }
        }

    }
}
