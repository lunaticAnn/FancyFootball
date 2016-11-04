using UnityEngine;
using System.Collections;

public class offense_event:bet_event{
	/*=================================bet event=======================================
	 * bet event is a class where we record all the details of a event in football game
	 * it include:
	 *- Time(float): during this quarter, this events happens at what time?
	 *- Question: question title?[do we really need this?]
	 *- Choices(string[3]? const string[3]?):
	 *- Ratio(float[3]): for the mock-up ratio chart
	 *- Team(string):
	=================================bet event=======================================*/
	public offense_event(bet_event _b){
		this.Time=_b.Time;
		this.ratio=_b.ratio;
		this.Answer=_b.Answer;
	}

	const string question="What is the next move?";
	
	public enum choices{run=0, throwball=1, whatever=2};

	public static readonly string[] event_name={"run","pass","QBRun"};

}
