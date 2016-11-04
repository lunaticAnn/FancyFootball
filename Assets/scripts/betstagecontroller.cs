using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class betstagecontroller : MonoBehaviour
{
    public static betstagecontroller instance = null;
    public GameObject[] control_panels;
    /* --0.team_choose_panel--
	 * --1.predict_panel---
	 * --2.wait_panel-----
	 * --3.outcome_panel--
	 * --4.reward_choosing--
	*/

    public enum betstages { choose_reward, predict, wait, outcome, choose_team };
    public List<bet_event> events_array;
    public const float bettimer = 3f; //timer
    public int user_choice;
    public float game_timer = 0f;
	public bool local_test;

    const float true_thread = 4f;
    // true thread is how long we start before a event really happens;

    private float game_timer_starts;
    private bool offense;
    private betstages current_stage;
    private int current_index;
    private float current_time_point = 0f;
    private float next_time_point;
    private int previous_answer;
	private float last_time_point;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

    }
    void Start()
    {
        //read the xml file for record, save it in events_list;
        //game_timer_starts=Time.time;
        //record when the game starts


        //WHENEVER offense bool is changed WRITE TO JSON FILE!
        if (!File.Exists(Path.Combine(Application.dataPath, "playerTeamChoice.json")))
        {
            offense = true;
            string data = offense.ToString();
            File.WriteAllText(Path.Combine(Application.dataPath, "playerTeamChoice.json"), data);
        }
        else
        {
            offense = bool.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "playerTeamChoice.json")));
        }
        //test
        EventsContainer events_list;
        events_list = xmlwriter.instance.Read_Events(offense);
        events_array = events_list.betting_events;
        current_index = 0;
        user_choice = -1;
        clear_panels();
        control_panels[0].SetActive(true);
		current_stage=betstages.choose_team;
        next_time_point = events_array[current_index].Time;
        current_time_point = 0f;
		last_time_point=events_array[events_array.Count-1].Time;
    }

	void fix_index(){
		//index-fixing for changing the game running time
		//find the first time point that:
		//next_time_point - true_thread - game_timer > 1e-7
		current_index = -1;
		//clear user choice;
		user_choice = -1;
		next_time_point = 0f;
		current_time_point=0f;
		while(next_time_point - true_thread < game_timer){
			current_index++;
			if(current_index>=events_array.Count){
				next_time_point=Mathf.Infinity;
				break;
			}
			next_time_point = events_array[current_index].Time;
		}
		Debug.Log("index fixing.."+current_index);

		int last_index=current_index-1;
		current_time_point=(last_index>=0)?events_array[last_index].Time:0f;

		_fixing=false;
	}

	bool _fixing=false;

    void Update()
    {
		if(local_test)
			game_timer=Time.time-game_timer_starts;
		
		else{
			float _time_temp=clientset.instance.ReturnGameTimer();
			game_timer =_time_temp >=0f?_time_temp:0f;
			}
        
		if (Mathf.Abs(next_time_point - true_thread - game_timer) < 0.1f){
			compel_invoke_bet();
		    }
		if (game_timer<last_time_point-true_thread)
			if(game_timer<current_time_point-true_thread||game_timer>next_time_point-true_thread)
				if(_fixing==false){
					_fixing=true;
					fix_index();
				    }
		      

    }

    public float action_time()
    {
        return current_time_point;
    }

    void clear_panels()
    {
        foreach (GameObject g in control_panels){
            g.SetActive(false);
        	}
    }

    void compel_invoke_bet()
    {
        if (current_index >= events_array.Count) { return; }
        clear_panels();
        control_panels[1].SetActive(true);
        user_choice = -1;
        control_panels[1].SendMessage("start_betting", events_array[current_index]);
        current_stage = betstages.predict;
        previous_answer = events_array[current_index].Answer;
        current_index += 1;
		current_time_point = next_time_point;

		if (current_index < events_array.Count){
            next_time_point = events_array[current_index].Time;
		}
		else next_time_point=Mathf.Infinity;
		Debug.Log("time for fun!"+next_time_point);

    }


    //activate each ui element acoording to the events
    public void switch_stage()
    {
        switch (current_stage)
        {
            case betstages.choose_team:
                clear_panels();
                control_panels[4].SetActive(true);
			    current_stage = betstages.choose_reward;
                return;
            case betstages.choose_reward:
                clear_panels();
                control_panels[1].SetActive(true);
				current_stage = betstages.predict;
				compel_invoke_bet();
                return;
            case betstages.predict:
                clear_panels();
                control_panels[2].SetActive(true);
                control_panels[2].SendMessage("myansweris", user_choice);
                current_stage = betstages.wait;
                return;
            case betstages.wait:
                clear_panels();
                control_panels[3].SetActive(true);
                control_panels[3].SendMessage("cheerup", user_choice == previous_answer);
                current_stage = betstages.outcome;
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
