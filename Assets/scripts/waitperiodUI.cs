using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class waitperiodUI : MonoBehaviour {

	private Text myanswer;
	// Use this for initialization
	void Awake () {
		myanswer=transform.GetChild(0).GetComponent<Text>();
	}

	void myansweris(int i){
		if(i==-1)myanswer.text="Oops, out of time.";
		else if(i>=0 &&i<=2)myanswer.text=offense_event.event_name[i];
		else	myanswer.text="?";
		StartCoroutine("countdown");
	}

	IEnumerator countdown(){
		while (betstagecontroller.instance.game_timer<betstagecontroller.instance.action_time())
		{yield return new WaitForEndOfFrame();}
		betstagecontroller.instance.switch_stage();
		yield return new WaitForEndOfFrame();
	}
}
