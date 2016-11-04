using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

/*Playercontainer contains functions includes:
 * save records(overwritten).
 * load records.
 * TODO: a simple sorting method for sorting events acoording to time;
*/

[XmlRoot("EventsCollection")]
public class EventsContainer
{
	[XmlArray("events"),XmlArrayItem("events")]
	public List<bet_event> betting_events;

	public void Save(string path)
	{
		Sort_my_self();

		var serializer = new XmlSerializer(typeof(EventsContainer));
		using(var stream = new FileStream(path, FileMode.Create))
		{
			serializer.Serialize(stream, this);
		}
	}

	public static EventsContainer Load(string path)
	{
		var serializer = new XmlSerializer(typeof(EventsContainer));
		using(var stream = new FileStream(path, FileMode.Open))
		{
			return serializer.Deserialize(stream) as EventsContainer;
		}
	}

    public bool delete_event(bet_event be, bool offense)
    {
        float threshold = 0.00001f;
        int idx = -1;
        for( int i = 0; i < betting_events.Count; ++i)
        {
            if (Mathf.Abs(betting_events[i].Time - be.Time) <= threshold)
                idx = i;
        }
        if (idx == -1)
            return false;

        betting_events.RemoveAt(idx);
        //if (betting_events.Remove(be) == false)
        //   UnityEngine.Debug.Log("fuck");

        string _path = offense ? xmlwriter.instance.get_offense_path() : xmlwriter.instance.get_defense_path();
        if (!File.Exists(_path))
        {
            return false;
        }
        Save(_path);
        return true;
    }

	private void Sort_my_self(){
		betting_events=betting_events.OrderBy(x=>x.Time).ToList();

        /*
		float[] sorting_index=new float[this.betting_events.length];
		for(int ind=0;ind<this.betting_events.length;ind++){
			sorting_index[ind]=this.betting_events[ind].Time;
		}
		System.Array.Sort(sorting_index,this.betting_events);
        */
    }
}
