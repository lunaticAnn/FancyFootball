using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SliderUISet : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler //{
{
    public static SliderUISet instance = null;
    public bool isDragged = false;
    public float time = 0.0f, tempTime;
    // Use this for initialization
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData data)
    {
        isDragged = true;
        tempTime = Time.time;
        Debug.Log("Pointer Down");
    }

    public void OnPointerUp(PointerEventData data)
    {
        time = tempTime;
        tempTime = 0;
        isDragged = false;
        Debug.Log("Pointer Up");
    }

    public void OnEndDrag(PointerEventData data)
    {
        time = tempTime;
        tempTime = 0;
        isDragged = false;
        Debug.Log("End Drag");
    }

    public void OnBeginDrag(PointerEventData data)
    {
        tempTime = Time.time;
        Debug.Log("Start Drag");
        isDragged = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
