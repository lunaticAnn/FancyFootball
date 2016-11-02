using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class developer_behave : MonoBehaviour {
	public Button submit_button;
	public Button clear_button;
    public Button delete_button;
	public Dropdown events;
	public ScrollRect content_checker;
	public InputField time_input;
	public bool Offense;
    public GameObject layoutButtonPrefab;
    public bet_event be_to_delete;

	/*=======================what developers do:==============================
	 * fill in the time;
	 * choose event;
	 * submit;
	 * once submitted, they are able to check the result they added in logging area;
	 =======================what developers do:==============================*/
	void Start () {
		submit_button.onClick.AddListener(addevent);
		clear_button.onClick.AddListener(delete_all);
        delete_button.onClick.AddListener(delegate { delete_one(be_to_delete); });

        update_content();
	}

	void addevent () {
		bet_event b=new bet_event();
		if(time_input.text==null){
			Debug.LogWarning("Please enter time");return;
			}
		b.Time=float.Parse(time_input.text);
		b.Answer=events.value;
		b.ratio=sphere_sampler();

		xmlwriter.instance.AddRecordToFile(Offense,b);
		update_content();

	}

	//random distribution for the voting
	float[] sphere_sampler(){
		Vector3 random_distribute=new Vector3(Random.value,Random.value,Random.value);
		random_distribute.Normalize();
		float[] dist=new float[3]{random_distribute.x*random_distribute.x,
			random_distribute.y*random_distribute.y,
			random_distribute.z*random_distribute.z};
		return dist;		
	}

	void update_content(){
        for(int i = 0; i < content_checker.transform.Find("Viewport").Find("Content").childCount; ++i)
        {
            Destroy(content_checker.transform.Find("Viewport").Find("Content").GetChild(i).gameObject);
        }

		EventsContainer ec=xmlwriter.instance.Read_Events(Offense);
		if(ec==null){
			content_checker.content.GetComponent<Text>().text="";
			return;
		}
        string content = "";
        foreach (bet_event b in ec.betting_events){
            content = "";
            content +=b.Time.ToString("F");
			content+="\n";
			for(int i=0;i<3;i++){
				content+="ratio "+i+" : "+b.ratio[i].ToString("P");
				content+="\n";
			}
			content+=offense_event.event_name[b.Answer];
			content+="\n";

            GameObject button = Instantiate(layoutButtonPrefab, Vector3.zero, Quaternion.identity, content_checker.transform.Find("Viewport").Find("Content")) as GameObject;
            button.transform.GetChild(0).GetComponent<Text>().text = content;
            button.GetComponent<eventdataholderUI>().be = b;
            button.GetComponent<Button>().onClick.AddListener(delegate { register_be_to_delete(button.GetComponent<eventdataholderUI>().be); });
        }
		//content_checker.content.GetComponent<Text>().text=content;
	}

	void delete_all(){
		xmlwriter.instance.delete_all(Offense);
		update_content();
	}

    void register_be_to_delete(bet_event be){
        be_to_delete = be;
    }

    void delete_one(bet_event be){
        EventsContainer ec = xmlwriter.instance.Read_Events(Offense);
        ec.delete_event(be, Offense);
        update_content();
    }
		
}
