using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class developer_behave : MonoBehaviour {
	public Button submit_button;
	public Button clear_button;
    public Button delete_button;
	public Dropdown events;
	public ScrollRect content_checker;
    public GameObject para_fields;
	public InputField time_input;
    public InputField[] para_input;
	public bool Offense;
    //prefabs
    public GameObject layoutButtonPrefab;
    public GameObject parafieldPrefab;
    //local storage
    public bet_event be_to_delete;

    //consts
    public float[] dist = {0.58f, 0.41f, 0.1f};

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

        para_input = new InputField[bet_event.bet_num_paras];
        for (int i = 0; i < para_input.Length; ++i)
        {
            GameObject parainputsingle = Instantiate(parafieldPrefab, Vector3.zero, Quaternion.identity, para_fields.transform) as GameObject;
            parainputsingle.transform.Find("Name").gameObject.GetComponent<Text>().text = bet_event.paraNames[i];
            para_input[i] = parainputsingle.GetComponent<InputField>();
            para_input[i].text = ((dist[i] * 100f)).ToString("F2");
        }

        update_content();
	}

	void addevent () {
		bet_event b = new bet_event();
		if(time_input.text.Length == 0){
			Debug.LogWarning("Please enter time");return;
		}
		b.Time=float.Parse(time_input.text);
		b.Answer=events.value;
		b.ratio= para_default_val();//set to default first, then check input
        float ratio_remaining = 1f;
        for(int i = 0; i < para_input.Length; ++i)
        {
            if (para_input[i].text.Length != 0) {
                b.ratio[i] = float.Parse(para_input[i].text) / 100f;
            }

            if ((ratio_remaining - b.ratio[i]) < 0)
                b.ratio[i] = ratio_remaining;
            ratio_remaining -= b.ratio[i];
        }

        Update_UI_Para_Input(b);

        xmlwriter.instance.AddRecordToFile(Offense,b);

		update_content();
	}

	//random distribution for the voting
	float[] sphere_sampler(){
		Vector3 random_distribute=new Vector3(Random.value,Random.value,Random.value);
		random_distribute.Normalize();
        //default value here
		float[] dist=new float[3]{random_distribute.x*random_distribute.x,
			random_distribute.y*random_distribute.y,
			random_distribute.z*random_distribute.z};
		return dist;		
	}

    float[] para_default_val()
    {
        float[] result = new float[dist.Length];
        for (int i = 0; i < dist.Length; ++i)
            result[i] = dist[i];
        return result;
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
        Update_UI_Para_Input(be);
    }

    void delete_one(bet_event be){
        EventsContainer ec = xmlwriter.instance.Read_Events(Offense);
        ec.delete_event(be, Offense);
        update_content();
    }

    void Update_UI_Para_Input(bet_event be)
    {
        for (int i = 0; i < para_input.Length; ++i)
        {
            para_input[i].text = ((be.ratio[i] * 100f)).ToString("F2");
        }
    }
		
}
