using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
	25.02.2021
*/
namespace Mkey
{
	public class OutTextMesh : MonoBehaviour
	{
		#region temp vars
		private TextMesh text;
        #endregion temp vars

		public void IntToText(int val)
        {
			SetText(val.ToString());
        }

		private void SetText(string newText)
        {
			if (!text) text = GetComponent<TextMesh>();
			if (text) text.text = newText;
        }
	}
}
