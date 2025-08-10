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
	public class OutText : MonoBehaviour
	{
		#region temp vars
		private Text text;
        #endregion temp vars

		public void IntToText(int val)
        {
			SetText(val.ToString());
        }

		private void SetText(string newText)
        {
			if (!text) text = GetComponent<Text>();
			if (text) text.text = newText;
        }
	}
}
