// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Windows;

namespace FancyZonesEditor.Models
{
    // CanvasLayoutModel
    //  Free form Layout Model, which specifies independent zone rects
    public class CanvasLayoutModel : LayoutModel
    {
        // Non-localizable strings
        public const string ModelTypeID = "canvas";

        // Default distance from the top and left borders to the zone.
        private const int DefaultOffset = 100;

        // Next created zone will be by OffsetShift value below and to the right of the previous.
        private const int OffsetShift = 50;

        // Zone size depends on the work area size multiplied by ZoneSizeMultiplier value.
        private const double ZoneSizeMultiplier = 0.4;

        // Distance from the top and left borders to the zone.
        private int _topLeft = DefaultOffset;

        public Rect CanvasRect { get; private set; }

        public CanvasLayoutModel(string uuid, string name, LayoutType type, IList<Int32Rect> zones, int width, int height)
            : base(uuid, name, type)
        {
            Zones = zones;
            TemplateZoneCount = Zones.Count;
            CanvasRect = new Rect(new Size(width, height));
        }

        public CanvasLayoutModel(string name, LayoutType type, int width, int height)
        : base(name, type)
        {
            CanvasRect = new Rect(new Size(width, height));
        }

        public CanvasLayoutModel(string name, LayoutType type)
        : base(name, type)
        {
        }

        public CanvasLayoutModel(string name)
        : base(name)
        {
        }

        public CanvasLayoutModel(CanvasLayoutModel other, bool enableQuickKeysPropertyChangedSubscribe = true)
            : base(other, enableQuickKeysPropertyChangedSubscribe)
        {
            CanvasRect = new Rect(other.CanvasRect.X, other.CanvasRect.Y, other.CanvasRect.Width, other.CanvasRect.Height);

            foreach (Int32Rect zone in other.Zones)
            {
                Zones.Add(zone);
            }
        }

        // Zones - the list of all zones in this layout, described as independent rectangles
        public IList<Int32Rect> Zones { get; private set; } = new List<Int32Rect>();

        // RemoveZoneAt
        //  Removes the specified index from the Zones list, and fires a property changed notification for the Zones property
        public void RemoveZoneAt(int index)
        {
            Zones.RemoveAt(index);
            TemplateZoneCount = Zones.Count;
            UpdateLayout();
        }

        // AddZone
        //  Adds the specified Zone to the end of the Zones list, and fires a property changed notification for the Zones property
        public void AddZone(Int32Rect zone)
        {
            Zones.Add(zone);
            TemplateZoneCount = Zones.Count;
            UpdateLayout();
        }

        public void AddZone()
        {
            AddNewZone();
            TemplateZoneCount = Zones.Count;
            UpdateLayout();
        }

        private void AddNewZone()
        {
            if (Zones.Count == 0)
            {
                _topLeft = DefaultOffset;
            }
            else if (_topLeft == Zones[Zones.Count - 1].X)
            {
                _topLeft += OffsetShift;
            }

            Rect workingArea = App.Overlay.WorkArea;
            int topLeft = (int)App.Overlay.ScaleCoordinateWithCurrentMonitorDpi(_topLeft);
            int width = (int)(workingArea.Width * ZoneSizeMultiplier);
            int height = (int)(workingArea.Height * ZoneSizeMultiplier);

            if (topLeft + width >= (int)workingArea.Width || topLeft + height >= (int)workingArea.Height)
            {
                _topLeft = DefaultOffset;
                topLeft = (int)App.Overlay.ScaleCoordinateWithCurrentMonitorDpi(_topLeft);
            }

            Zones.Add(new Int32Rect(topLeft, topLeft, width, height));
            _topLeft += OffsetShift;
        }

        // InitTemplateZones
        // Creates zones based on template zones count
        public override void InitTemplateZones()
        {
            if (Type == LayoutType.Custom || Type == LayoutType.Blank)
            {
                return;
            }

            Zones.Clear();
            for (int i = 0; i < TemplateZoneCount; i++)
            {
                AddNewZone();
            }

            TemplateZoneCount = Zones.Count;
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            FirePropertyChanged();
        }

        // Clone
        //  Implements the LayoutModel.Clone abstract method
        //  Clones the data from this CanvasLayoutModel to a new CanvasLayoutModel
        public override LayoutModel Clone()
        {
            CanvasLayoutModel layout = new CanvasLayoutModel(Name);

            foreach (Int32Rect zone in Zones)
            {
                layout.Zones.Add(zone);
            }

            layout.SensitivityRadius = SensitivityRadius;
            layout.CanvasRect = CanvasRect;
            return layout;
        }

        public void RestoreTo(CanvasLayoutModel other)
        {
            other.Zones.Clear();
            foreach (Int32Rect zone in Zones)
            {
                other.Zones.Add(zone);
            }

            other._topLeft = _topLeft;
            other.SensitivityRadius = SensitivityRadius;
            other.CanvasRect = CanvasRect;
            other.UpdateLayout();
        }

        // PersistData
        // Implements the LayoutModel.PersistData abstract method
        protected override void PersistData()
        {
            AddCustomLayout(this);
        }
    }
}
