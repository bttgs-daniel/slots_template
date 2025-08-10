using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
/*
 02.01.2021
 */
namespace Mkey
{
    //http://answers.unity3d.com/questions/1086415/gradient-text-in-unity-522-basevertexeffect-is-obs.html?childToView=1103637#answer-1103637
    public class UIGradient : BaseMeshEffect
    {
        [SerializeField]
        GType _gradientType;

        [SerializeField]
        GBlend _blendMode = GBlend.Multiply;

        [SerializeField]
        [Range(-1, 1)]
        float _offset = 0f;

        [SerializeField]
        UnityEngine.Gradient _effectGradient = new UnityEngine.Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) } };

        #region Properties
        public GBlend BlendMode
        {
            get { return _blendMode; }
            set { _blendMode = value; }
        }

        public UnityEngine.Gradient EffectGradient
        {
            get { return _effectGradient; }
            set { _effectGradient = value; }
        }

        public GType GradientType
        {
            get { return _gradientType; }
            set { _gradientType = value; }
        }

        public float Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }
        #endregion

        public override void ModifyMesh(VertexHelper helper)
        {
            if (!IsActive() || helper.currentVertCount == 0)
                return;

            List<UIVertex> _vertexList = new List<UIVertex>();

            helper.GetUIVertexStream(_vertexList);

            int nCount = _vertexList.Count;
            switch (GradientType)
            {
                case GType.Horizontal:
                    {
                        float left = _vertexList[0].position.x;
                        float right = _vertexList[0].position.x;
                        float x = 0f;

                        for (int i = nCount - 1; i >= 1; --i)
                        {
                            x = _vertexList[i].position.x;

                            if (x > right) right = x;
                            else if (x < left) left = x;
                        }

                        float width = 1f / (right - left);
                        UIVertex vertex = new UIVertex();

                        for (int i = 0; i < helper.currentVertCount; i++)
                        {
                            helper.PopulateUIVertex(ref vertex, i);

                            vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.x - left) * width - Offset));

                            helper.SetUIVertex(vertex, i);
                        }
                    }
                    break;

                case GType.Vertical:
                    {
                        float bottom = _vertexList[0].position.y;
                        float top = _vertexList[0].position.y;
                        float y = 0f;

                        for (int i = nCount - 1; i >= 1; --i)
                        {
                            y = _vertexList[i].position.y;

                            if (y > top) top = y;
                            else if (y < bottom) bottom = y;
                        }

                        float height = 1f / (top - bottom);
                        UIVertex vertex = new UIVertex();

                        for (int i = 0; i < helper.currentVertCount; i++)
                        {
                            helper.PopulateUIVertex(ref vertex, i);

                            vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.y - bottom) * height - Offset));

                            helper.SetUIVertex(vertex, i);
                        }
                    }
                    break;
            }
        }

        private Color BlendColor(Color colorA, Color colorB)
        {
            switch (BlendMode)
            {
                default: return colorB;
                case GBlend.Add: return colorA + colorB;
                case GBlend.Multiply: return colorA * colorB;
            }
        }

        public enum GType
        {
            Horizontal,
            Vertical
        }

        public enum GBlend
        {
            Override,
            Add,
            Multiply
        }
    }
}