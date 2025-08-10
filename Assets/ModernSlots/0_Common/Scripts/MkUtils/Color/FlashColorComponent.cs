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
	public class FlashColorComponent : MonoBehaviour
	{
		public Color fromColor = Color.white;
		public Color toColor = Color.red;
		public bool flashChildren = false;
		public float period = 1;
		public bool flashAtStart = false;

		#region temp vars
		ColorFlasher cF;
        #endregion temp vars
		
		
		#region regular
		private void Start()
		{
			if (flashChildren)
			{
				TextMesh[] tms = GetComponentsInChildren<TextMesh>();
				Image[] ims = GetComponentsInChildren<Image>();
				SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
				Text[] txs = GetComponentsInChildren<Text>();

				cF = new ColorFlasher(gameObject, tms, txs, srs, ims, period);
			}
            else
            {
				TextMesh tm = GetComponent<TextMesh>();
				TextMesh[] tms = (tm) ? new TextMesh[] { tm } : null;

				Image im = GetComponent<Image>();
				Image[] ims = (im) ? new Image[] { im } : null;

				SpriteRenderer sr = GetComponent<SpriteRenderer>();
				SpriteRenderer [] srs = (sr) ? new SpriteRenderer[] { sr } : null;

				Text tx = GetComponent<Text>();
				Text[] txs = (tx) ? new Text[] { tx } : null;

				cF = new ColorFlasher(gameObject, tms, txs, srs, ims, period);
			}

			if (flashAtStart) StartFlashing();
		}
		#endregion regular

		public void StartFlashing()
        {
			if (cF != null)
				cF.Flashing(fromColor, toColor); //cF.FlashingAlpha(); //cF.Flashing((sc, t)=> { return new Color(sc.r, sc.g * t, sc.b * t, sc.a); }); 
		}

		public void Cancel()
        {
			if (cF != null) cF.Cancel();
		}
	}
}
/*mkutils*/