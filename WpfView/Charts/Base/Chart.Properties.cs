﻿//The MIT License(MIT)

//copyright(c) 2016 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts.Charts;

namespace LiveCharts.Wpf.Charts.Base
{
    public abstract partial class Chart
    {
        /// <summary>
        /// The DataClick event is fired when a user click any data point
        /// </summary>
        public event Action<object, ChartPoint> DataClick;

        private static Random Randomizer { get; set; }
        private SeriesCollection LastKnownSeriesCollection { get; set; }

        /// <summary>
        /// Gets or sets the chart current canvas
        /// </summary>
        protected Canvas Canvas { get; set; }
        internal Canvas DrawMargin { get; set; }
        internal int SeriesIndexCount { get; set; }
        
        /// <summary>
        /// Gets or sets whether charts must randomize the starting default series color.
        /// </summary>
        public static bool RandomizeStartingColor { get; set; }
        /// <summary>
        /// Gets or sets the default series color set.
        /// </summary>
        public static List<Color> Colors { get; set; }
       
        public static readonly DependencyProperty AxisYProperty = DependencyProperty.Register(
            "AxisY", typeof(AxesCollection), typeof(Chart),
            new PropertyMetadata(null, CallChartUpdater()));
        /// <summary>
        /// Gets or sets vertical axis
        /// </summary>
        public AxesCollection AxisY
        {
            get { return (AxesCollection)GetValue(AxisYProperty); }
            set { SetValue(AxisYProperty, value); }
        }

        public static readonly DependencyProperty AxisXProperty = DependencyProperty.Register(
            "AxisX", typeof(AxesCollection), typeof(Chart),
            new PropertyMetadata(null, CallChartUpdater()));
        /// <summary>
        /// Gets or sets horizontal axis
        /// </summary>
        public AxesCollection AxisX
        {
            get { return (AxesCollection)GetValue(AxisXProperty); }
            set { SetValue(AxisXProperty, value); }
        }

        public static readonly DependencyProperty ChartLegendProperty = DependencyProperty.Register(
            "ChartLegend", typeof(DefaultLegend), typeof(Chart),
            new PropertyMetadata(default(DefaultLegend), CallChartUpdater()));
        /// <summary>
        /// Gets or sets the control to use as chart legend for this chart.
        /// </summary>
        public DefaultLegend ChartLegend
        {
            get { return (DefaultLegend)GetValue(ChartLegendProperty); }
            set { SetValue(ChartLegendProperty, value); }
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom", typeof(ZoomingOptions), typeof(Chart),
            new PropertyMetadata(default(ZoomingOptions)));
        /// <summary>
        /// Gets or sets chart zoom behavior
        /// </summary>
        public ZoomingOptions Zoom
        {
            get { return (ZoomingOptions)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public static readonly DependencyProperty LegendLocationProperty = DependencyProperty.Register(
            "LegendLocation", typeof(LegendLocation), typeof(Chart),
            new PropertyMetadata(LegendLocation.None, CallChartUpdater()));
        /// <summary>
        /// Gets or sets where legend is located
        /// </summary>
        public LegendLocation LegendLocation
        {
            get { return (LegendLocation)GetValue(LegendLocationProperty); }
            set { SetValue(LegendLocationProperty, value); }
        }

        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
            "Series", typeof(SeriesCollection), typeof(Chart),
            new PropertyMetadata(default(SeriesCollection), OnSeriesChanged));

        /// <summary>
        /// Gets or sets chart series collection to plot.
        /// </summary>
        public SeriesCollection Series
        {
            get { return (SeriesCollection)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value);}
        }

        public static readonly DependencyProperty AnimationsSpeedProperty = DependencyProperty.Register(
            "AnimationsSpeed", typeof(TimeSpan), typeof(Chart),
            new PropertyMetadata(default(TimeSpan), UpdateChartFrequency));

        /// <summary>
        /// Gets or sets the default animation speed for this chart, you can override this speed for each element (series and axes)
        /// </summary>
        public TimeSpan AnimationsSpeed
        {
            get { return (TimeSpan)GetValue(AnimationsSpeedProperty); }
            set { SetValue(AnimationsSpeedProperty, value); }
        }

        public static readonly DependencyProperty DisableAnimationsProperty = DependencyProperty.Register(
            "DisableAnimations", typeof (bool), typeof (Chart),
            new PropertyMetadata(default(bool), UpdateChartFrequency));
        /// <summary>
        /// Gets or sets if the chart is animated or not.
        /// </summary>
        public bool DisableAnimations
        {
            get { return (bool)GetValue(DisableAnimationsProperty); }
            set { SetValue(DisableAnimationsProperty, value); }
        }

        public static readonly DependencyProperty DataTooltipProperty = DependencyProperty.Register(
            "DataTooltip", typeof(DefaultTooltip), typeof(Chart), new PropertyMetadata(default(DefaultTooltip)));
        /// <summary>
        /// Gets or sets the chart data tooltip.
        /// </summary>
        public DefaultTooltip DataTooltip
        {
            get { return (DefaultTooltip)GetValue(DataTooltipProperty); }
            set { SetValue(DataTooltipProperty, value); }
        }

        public static readonly DependencyProperty HoverableProperty = DependencyProperty.Register(
            "Hoverable", typeof(bool), typeof(Chart), new PropertyMetadata(true));
        /// <summary>
        /// gets or sets whether chart should react when a user moves the mouse over a data point.
        /// </summary>
        public bool Hoverable
        {
            get { return (bool)GetValue(HoverableProperty); }
            set { SetValue(HoverableProperty, value); }
        }
        /// <summary>
        /// Gets the chart model, the model is who calculates everything, is the engine of the chart
        /// </summary>
        public ChartCore Model
        {
            get { return ChartCoreModel; }
        }

        /// <summary>
        /// Gets whether the chart has an active tooltip.
        /// </summary>
        public bool HasTooltip
        {
            get { return DataTooltip != null; }
        }

        /// <summary>
        /// Gets whether the chart has a DataClick event attacked.
        /// </summary>
        public bool HasDataClickEventAttached
        {
            get { return DataClick != null; }
        }

        /// <summary>
        /// Gets whether the chart is already loaded in the view.
        /// </summary>
        public bool IsControlLoaded { get; private set; }
        
    }
}
