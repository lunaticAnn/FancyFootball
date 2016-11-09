using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class scorestatus : MonoBehaviour {

	public Text score_text;
	public Text percentage_text;
	public Image reward_sprite;
	public Sprite[] reward_src;
	public Image progression_bar;

	private int reward_chosen=-1;
	private int current_score=0;
	private float percentage;
	bool has_init=false;


	void set_reward (rewardUI.reward_ind Rmsg) {
		//this will set the reward choosen to be r;
		//change the image for the prize to sprite;

		reward_chosen=Rmsg.rewardpoints;
		reward_sprite.sprite=reward_src[Rmsg.reward_index];
		has_init=true;
	}
	

	void Update () {
		if(has_init){
			score_text.text=current_score+" Points";
			percentage=current_score*1f/reward_chosen;
			percentage_text.text=percentage.ToString("P")+" to ";
			progression_bar.fillAmount=percentage<1f?percentage:1f;
			progression_bar.color=percentage<1f?Color.white:Color.red;
		}
	}

	void get_score(int s){
		current_score+=s;
	}
		
}
