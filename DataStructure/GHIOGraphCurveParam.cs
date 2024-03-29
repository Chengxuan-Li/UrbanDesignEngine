﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Display;
using GH_IO.Serialization;
using Rhino.Render;

namespace UrbanDesignEngine.DataStructure
{
    public class GHIOGraphCurveParam<ScriptVariable> : GHIOPreviewGeometricParam<ScriptVariable, Curve> where ScriptVariable : IHasGeometry<Curve>
    {
        public override Action<GH_PreviewWireArgs, Curve> DrawWires => (a, g) => a.Pipeline.DrawCurve(g, PreviewSettings.GraphPreviewColor, PreviewSettings.Thickness);

        public override Action<GH_PreviewMeshArgs, Curve> DrawMeshes => (a, g) => a.Pipeline.DrawCurve(g, PreviewSettings.GraphPreviewColor, PreviewSettings.Thickness);

    }

    public class GHIOTensorFieldCurvesParam<ScriptVariable> : GHIOPreviewGeometryListParam<ScriptVariable, Curve> where ScriptVariable : IHasGeometryList<Curve>
    {
        public override Action<GH_PreviewWireArgs, Curve> DrawWires => (a, g) => { };

        public override Action<GH_PreviewMeshArgs, Curve> DrawMeshes => (a, g) => a.Pipeline.DrawLineArrow(new Line(g.PointAtStart, g.PointAtEnd), PreviewSettings.TensorFieldPreviewColor, PreviewSettings.Thickness, PreviewSettings.ArrowSize);

    }
}
