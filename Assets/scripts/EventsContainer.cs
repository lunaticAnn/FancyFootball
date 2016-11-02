using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Xml;

/*Playercontainer contains functions includes:
 * save records(overwritten).
 * load records.
 * TODO: a simple sorting method for sorting events acoording to time;
*/

[XmlRoot("EventsCollection")]
public class EventsContainer
{
	[XmlArray("events"),XmlArrayItem("events")]
	public bet_event[] betting_events;

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
	private void Sort_my_self(){
		float[] sorting_index=new float[this.betting_events.Length];
		for(int ind=0;ind<this.betting_events.Length;ind++){
			sorting_index[ind]=this.betting_events[ind].Time;
		}
		System.Array.Sort(sorting_index,this.betting_events);
	}
}
