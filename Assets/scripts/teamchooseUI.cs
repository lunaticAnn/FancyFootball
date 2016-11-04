using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class teamchooseUI : MonoBehaviour {

	private Button steeler;
	void Start () {
		steeler=transform.GetChild(0).GetComponent<Button>();
		steeler.onClick.AddListener(jump_stage);
	}


	void jump_stage() {
		betstagecontroller.instance.switch_stage();	
	}
}
