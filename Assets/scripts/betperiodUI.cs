using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class betperiodUI : MonoBehaviour {

	private Image timer_circle;
	private GameObject[] my_choices;
	private GameObject[] info_chart;

	/*----------UI child index-------------
	 * 0.choices
	 * 1.timer
	 * 2.informations
	---------------------------------------*/
	//all choices ui which includes the ratio graph and ratio text;
	const int num_of_choices=3;
	const int num_of_info=3;

	// Use this for initialization
	void Awake () {
		init();
	}

	void init(){
		timer_circle=transform.GetChild(1).GetChild(0).GetComponent<Image>();
		my_choices=new GameObject[num_of_choices];
		info_chart=new GameObject[num_of_info];
		for(int i=0;i<num_of_choices;i++){
			my_choices[i]=transform.GetChild(0).GetChild(i).gameObject;
			info_chart[i]=transform.GetChild(2).GetChild(i).gameObject;
			Button this_button=my_choices[i].GetComponent<Button>();
			int val=i;
			this_button.onClick.AddListener(delegate{submit_answer(val);});
		}

	}

	void submit_answer(int i){
		betstagecontroller.instance.user_choice=i;
		betstagecontroller.instance.switch_stage();
	}

	void start_betting(bet_event b){
		/*
		 * what to do:
		 * start timer();
		 * show_graph;
		*/
		IEnumerator c=timer_ui(betstagecontroller.bettimer);
		StartCoroutine(c);
		for (int i=0;i<num_of_choices;i++){
			c=ratio_paint(i,b.ratio[i]);
			Debug.Log(b.ratio[i]);
			StartCoroutine(c);
		}
	}

	const int frame_rate=30;
	IEnumerator timer_ui(float t){
		timer_circle.fillAmount=1f;
		int thread=(int)(frame_rate*t);
		for(int i=0;i<thread;i++){
			float delta=1f/(frame_rate*t);
			if(timer_circle.fillAmount>=0f)
			timer_circle.fillAmount-=delta;
			yield return new WaitForEndOfFrame();
		}
		betstagecontroller.instance.switch_stage();
	}

	const float paint_delta=0.02f;
	IEnumerator ratio_paint(int index, float r){
		Image ratio_chart=info_chart[index].transform.GetChild(0).GetComponent<Image>();
		Text ratio_text=info_chart[index].transform.GetChild(1).GetComponent<Text>();
		ratio_chart.fillAmount=0;
		float ratio=0;
		while(ratio<r){
			ratio+=paint_delta;
			ratio_chart.fillAmount=ratio;
			ratio_text.text=ratio.ToString("P");
			yield return new WaitForEndOfFrame();
		}
		ratio_chart.fillAmount=r;
		ratio_text.text=r.ToString("P");
	}

	

}
