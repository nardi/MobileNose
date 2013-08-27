using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DroidNose
{
    public class BorderedLinearLayout : LinearLayout
    {
        private bool leftBorder = true,
                     topBorder = true,
                     bottomBorder = true,
                     rightBorder = true;
        private readonly int borderSize = 1;
        private Paint Paint = new Paint();

        public BorderedLinearLayout(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public BorderedLinearLayout(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetWillNotDraw(false);
            Paint.Alpha = 0xAA;
            Paint.SetStyle(Paint.Style.Stroke);
            Paint.Color = Color.LightGray;
            Paint.StrokeWidth = borderSize * 2;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (topBorder)
            {
                canvas.DrawLine(0, borderSize, Width, borderSize, Paint);
            }
            if (leftBorder)
            {
                canvas.DrawLine(borderSize, topBorder ? borderSize : 0, borderSize, Height, Paint);
            }
            if (rightBorder)
            {
                canvas.DrawLine(Width - borderSize, topBorder ? borderSize : 0, Width - borderSize, Height, Paint);
            }
            if (bottomBorder)
            {
                canvas.DrawLine(leftBorder ? borderSize : 0, Height - borderSize,
                                Width - (leftBorder ? borderSize : 0), Height - borderSize, Paint);
            }
        }
    }
}