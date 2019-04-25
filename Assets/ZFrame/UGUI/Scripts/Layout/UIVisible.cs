using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI {

	public class UIVisible : MonoBehaviour {
	    
	    private void setAlpha(float alpha)
	    {
	        var cv = GetComponent<CanvasGroup>();
	        if (cv) {
	            cv.alpha = alpha;
	            return;
	        }

	        var graphic = GetComponent<UnityEngine.UI.Graphic>();
	        if (graphic) {
	            var c = graphic.color;
	            graphic.color = new Color(c.r, c.g, c.b, alpha);
	        }
	    }

	    public void SyncVisible(bool visible)
	    {
	        setAlpha(visible ? 1 : 0);
	    }

	    public void AntiVisible(bool visible)
	    {
	        setAlpha(visible ? 0 : 1);
	    }

	    public void SyncActive(bool active)
	    {
	        gameObject.SetActive(active);
	    }

	    public void AntiActive(bool active)
	    {
	        gameObject.SetActive(!active);
	    }

	}
}
