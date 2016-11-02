using UnityEngine;
using System.Collections;

public class betstagecontroller : MonoBehaviour {
	public static betstagecontroller instance=null;
	public GameObject[] control_panels;
	/* --0.reward_choose_panel--
	 * --1.predict_panel---
	 * --2.wait_panel-----
	 * --3.outcome_panel--
	 * --4.team_choosing--
	*/

	public enum betstages{choose_reward,predict,wait,outcome,choose_team};
	public bet_event[] events_array;
	public const float bettimer=3f; //timer
	public int user_choice;
	public float game_timer=0f;

	const float true_thread=4f; 
	// true thread is how long we start before a event real happens;

	private float game_timer_starts;
	private bool offense;
	private betstages current_stage;
	private int current_index;
	private float current_time_point=0f;
	private float next_time_point;
	private int previous_answer;

	void Awake(){
		if(instance==null)instance=this;
		else Destroy(gameObject);

	}
	void Start () {
		//read the xml file for record, save it in events_list;
		game_timer_starts=Time.time;
		//record when the game starts
		offense=true;
		//test
		EventsContainer events_list;
		events_list=xmlwriter.instance.Read_Events(offense);
		events_array=events_list.betting_events;
		current_index=0;
		user_choice=-1;
		clear_panels();
		control_panels[4].SetActive(true);
		next_time_point=events_array[current_index].Time;
		current_time_point=0f;
	}
		
	void Update(){
		if(Input.GetKeyDown(KeyCode.Space)){
			switch_stage();
		}
		game_timer=Time.time-game_timer_starts;
		if(next_time_point-true_thread-game_timer<1e-7){
			compel_invoke_bet();
		}

	}

	public float action_time(){
		return current_time_point;
	}

	void clear_panels(){
		foreach(GameObject g in control_panels){
			g.SetActive(false);
		}
	}

	void compel_invoke_bet(){
		if(current_index>=events_array.Length){return;}
		clear_panels();
		control_panels[1].SetActive(true);
		user_choice=-1;
		control_panels[1].SendMessage("start_betting",events_array[current_index]);
		current_stage=betstages.predict;
		previous_answer=events_array[current_index].Answer;
		current_index+=1;
		current_time_point=next_time_point;
		if(current_index<events_array.Length)
		next_time_point=events_array[current_index].Time;
		Debug.Log("time for fun!");

	}


	//activate each ui element acoording to the events
	public void switch_stage () {
		switch(current_stage){
		case betstages.choose_team:
			clear_panels();
			control_panels[0].SetActive(true);
			return;
		case betstages.choose_reward:
			clear_panels();
			control_panels[1].SetActive(true);
			return;
		case betstages.predict:
			clear_panels();
			control_panels[2].SetActive(true);
			control_panels[2].SendMessage("myansweris",user_choice);
			current_stage=betstages.wait;
			return;
		case betstages.wait:
			clear_panels();
			control_panels[3].SetActive(true);
			control_panels[3].SendMessage("cheerup",user_choice==previous_answer);
			current_stage=betstages.outcome;
			return;
		case betstages.outcome:
			clear_panels();
			control_panels[1].SetActive(true);
			return;
		default:
			return;
		}
	}
}
