using System.Xml;
using System.Xml.Serialization;

public class bet_event
{
	/*
	 * players class right now contains:
	 * name : player name
	 * description: the system time when the certain card is got
	 * img_link: the url which linked to the player's featured image that is used to compose the card. 
	*/
	[XmlAttribute("Time")]
	public float Time;

	[XmlAttribute("Ratio")]
	public float[] ratio;

	[XmlAttribute("Answer")]
	public int Answer;
}
