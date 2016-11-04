using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class rewardUI : MonoBehaviour {
	static int[] sprite_index={0,1,2};
	static int[] reward={50,100,200};

	public struct reward_ind{
		public int rewardpoints;
		public int reward_index;
	}


	void Start () {
		for(int i=0;i<transform.childCount;i++){
			Button this_button=transform.GetChild(i).GetComponent<Button>();
			int val=i;
			this_button.onClick.AddListener(delegate{submit_reward(val);});
		}
	}
	

	void submit_reward (int i) {
		reward_ind r=new reward_ind();
		r.rewardpoints=reward[i];
		r.reward_index=sprite_index[i];
		GameObject.Find("AlwaysHere").SendMessage("set_reward",r);
		betstagecontroller.instance.switch_stage();
	}
}
