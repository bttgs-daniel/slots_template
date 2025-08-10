using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class ChangeTextColor : MonoBehaviour
    {
        [SerializeField]
        private Color color1;
        [SerializeField]
        private Color color2;

        private Text text;
        private TextMesh textMesh;
        private LineBehavior line;

        void Start()
        {
            text = GetComponent<Text>();
            textMesh = GetComponent<TextMesh>();
            line = GetComponentInParent<LineBehavior>();
            SetColor1();
            if(line) line.ChangeSelectionEvent += (s) => { if (s) SetColor2(); else SetColor1(); };
            if (line && line.IsSelected) SetColor2();
        }

        private void SetColor(Color color)
        {
            if (text) text.color = color;
            if (textMesh) textMesh.color = color;
        }

        public void SetColor1()
        {
            SetColor(color1);
        }

        public void SetColor2()
        {
            SetColor(color2);
        }
    }
}