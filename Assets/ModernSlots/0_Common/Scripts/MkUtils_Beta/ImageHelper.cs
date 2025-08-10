using UnityEngine;
using UnityEngine.UI;

/*
	helper script to change image opacity
	28.04.2021
 */
namespace Mkey
{
	public class ImageHelper : MonoBehaviour
	{
		[SerializeField]
		private Color32 color;

		public void SetAlpha(float alpha)
        {
			Image im = GetComponent<Image>();
			if (im) im.color = new Color(im.color.r, im.color.r, im.color.b, Mathf.Clamp01(alpha));
        }

		public void SetAlphaWithChildren(float alpha)
		{
			Image [] ims = GetComponentsInChildren<Image>();
            foreach (var im in ims)
            {
				if (im) im.color = new Color(im.color.r, im.color.r, im.color.b, Mathf.Clamp01(alpha));
			}
		}

		public void SetColor()
		{
			Image im = GetComponent<Image>();
			if (im) im.color = color;
		}

		public void SetColorWithChildren()
		{
			Image[] ims = GetComponentsInChildren<Image>();
			foreach (var im in ims)
			{
				if (im) im.color = color;
			}
		}

		public void SetColor(float delay)
		{
			TweenExt.DelayAction(gameObject, delay, ()=>
			{
				SetColor();
			});
		}

		public void SetColorWithChildren(float delay)
		{
			TweenExt.DelayAction(gameObject, delay, () =>
			{
				SetColorWithChildren();
			});
		}
	}
}
