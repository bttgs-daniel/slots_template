using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    12.08.2020 - first
    17.02.2021 - add setactive , setinvactive
    02.03.2021 - SetActiveTrue ...
 */
namespace Mkey
{
    public class GameObjectToggle : MonoBehaviour
    {
        public void Toggle()
        {
          if(gameObject) gameObject.SetActive(!isActiveAndEnabled);
        }

        public void SetActive(bool active)
        {
            if (gameObject) gameObject.SetActive(active);
        }

        public void SetInvActive(bool active)
        {
            if (gameObject) gameObject.SetActive(!active);
        }

        public void SetSprite(Sprite s)
        {
            Image im = GetComponent<Image>();
            if (im)
            {
                im.sprite = s;
                return;
            }
            SpriteRenderer sr= GetComponent<SpriteRenderer>();
            if (sr) sr.sprite = s;
        }

        public void SetActiveTrue()
        {
            if (gameObject) gameObject.SetActive(true);
        }

        public void SetActiveFalse()
        {
            if (gameObject) gameObject.SetActive(false);
        }

        public void SetActiveTrueDelay(float delay)
        {
            Invoke("SetActiveTrue", delay);
        }

        public void SetActiveFalseDelay(float delay)
        {
            Invoke("SetActiveFalse", delay);
        }
    }
}