using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class outcomeUI : MonoBehaviour {
	readonly string[] win_words={"WoW!","O(∩_∩)O","↖(^ω^)↗"};
	readonly string[] lose_words={"Hmm..","_(:з」∠)_","(╯﹏╰)"};

	private Text cheerT;
	
	// Update is called once per frame
	void cheerup (bool correct_answer) {
		cheerT=transform.GetChild(0).GetComponent<Text>();
		string cheer_text = correct_answer?shuffle_str(win_words):shuffle_str(lose_words);
		cheerT.text=cheer_text;
	}

	string shuffle_str(string[] s){
		int i=s.Length;
		int seed=Random.Range(0,i);
		return s[seed];
	}
}
