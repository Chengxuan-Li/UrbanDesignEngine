using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace UrbanDesignEngine.DataStructure
{
    public class Attributes : IAttributable
    {
        public GHIOParam<Attributes> GHIOParam => new GHIOParam<Attributes>(this);
        public Guid Guid = default;
        public GeometryBase Geometry = default;
        public Dictionary<string, object> Content = new Dictionary<string, object>();

        public bool Contains(string key)
        {
            return Content.ContainsKey(key);
        }

        public void SetUnderlyingCurve(Curve c)
        {
            Set("UnderlyingCurve", c);
        }

        public void SetGeometry(GeometryBase geo)
        {
            Geometry = geo;
            return;
        }

        public void SetGeometry(Curve c)
        {
            Geometry = c;
            return;
        }

        public void SetGeometry(Line line)
        {
            Geometry = line.ToNurbsCurve();
            return;
        }

        public void SetGeometry(Point3d pt)
        {
            Geometry = new Point(pt);
            return;
        }

        public void SetGeometry(Polyline pl)
        {
            Geometry = pl.ToNurbsCurve();
            return;
        }

        public void Set(string key, object val)
        {
            if (Content.ContainsKey(key))
            {
                Content[key] = val;
            } else
            {
                Content.Add(key, val);
            }
        }
        
        public T Get<T>(string key)
        {
            T val;
            TryGet<T>(key, out val);
            return val;
        }

        public bool TryGet<T>(string key, out T val)
        {
            val = default;
            if (Content.ContainsKey(key))
            {
                val = (T)Content[key];
                return !(val == null);
            } else
            {
                return false;
            }
        }

        public Attributes Duplicate()
        {
            Attributes attributes = new Attributes() { Content = Content.ToDictionary(entry => entry.Key, entry => entry.Value) };
            attributes.Guid = Guid;
            attributes.Geometry = Geometry;
            return attributes;
        }

        public Attributes GetAttributesInstance()
        {
            return this;
        }

        public void SetAttribute(string key, object val)
        {
            Set(key, val);
        }

        public T GetAttribute<T>(string key)
        {
            return Get<T>(key);
        }

        public bool TryGetAttribute<T>(string key, out T val)
        {
            return TryGet<T>(key, out val);
        }

    }
}
