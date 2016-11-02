using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class xmlwriter : MonoBehaviour {

	/*A singleton which will stay consistent among scenes
	 * it contains functions of:
	 * AddRecordToFile(): Players you already got
	 
	*/
	public static xmlwriter instance=null;



	string offense_path;
	string defense_path;
	string root_path;

	void Awake(){
		if(instance==null){instance=this;}
		else
		{Destroy(this.gameObject);}
		DontDestroyOnLoad(gameObject);

		#if UNITY_STANDALONE||UNITY_WEBPLAYER||UNITY_EDITOR
		offense_path=Path.Combine(Application.dataPath,"offense_events.xml");
		defense_path=Path.Combine(Application.dataPath,"defense_events.xml");
		root_path=Application.dataPath;

		#elif UNITY_IOS || UNITY_ANDROID ||UNITY_WP8 || UNITY_IPHONE
		offense_path=Path.Combine(Application.persistentDataPath,"offense_events.xml");
		defense_path=Path.Combine(Application.persistentDataPath,"defense_events.xml");
		root_path=Application.persistentDataPath;
		#endif
	}



	public void AddRecordToFile(bool offense, bet_event _be)
	{
		string _path=offense? offense_path:defense_path;
		if(!File.Exists(_path)){
			EventsContainer new_events=new EventsContainer();
			new_events.betting_events=new bet_event[1];
			new_events.betting_events[0]=_be;
			new_events.Save(_path);
			return;
			}

		EventsContainer current_events=EventsContainer.Load(_path);
		int pre_alloc=current_events.betting_events.GetLength(0);
		bet_event[] new_copy=new bet_event[pre_alloc+1];
		for(int index=0;index<pre_alloc;index++){
			new_copy[index]=current_events.betting_events[index];
		}
		new_copy[pre_alloc]=_be;
		current_events.betting_events=new_copy;
		current_events.Save(_path);
	}


	public EventsContainer Read_Events(bool offense){
		string _path=offense? offense_path:defense_path;
		if(File.Exists(_path)){
			return EventsContainer.Load(_path);
		}
		else
			return null;
	}



	public void delete_all(bool offense){
		string _path=offense? offense_path:defense_path;
		//delete data
		if(File.Exists(_path)){
			File.Delete(_path);
		}
	}
		

}
