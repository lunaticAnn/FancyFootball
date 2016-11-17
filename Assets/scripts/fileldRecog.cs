using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;// for calling external program
using UnityEngine.UI;

public class fileldRecog : MonoBehaviour {

	struct Vector2Int{
		public int x;
		public int y;
		public void init (int a, int b){
			x = a; y = b;
		}
	}

    Color color_ref = new Color(75f/255f, 111f/255f, 68f/255f);

    const float FieldLength = 109.728f;
    const float FieldWidth = 48.768f;
    Renderer rend;
	public GameObject refcamera;
    Texture2D texture;
    Texture2D textureMask;//the mask used t
    Color[] colors;
	Color[] origColors;//this one not get changed after copy
    float default_offset = 20f / 256f;
    public GameObject field;
    public Quaternion phoneQuat;
	public Text debugText;
	public GameObject rotationReference;
	WebCamTexture webcamtexture;
	int width = 256, height = 144;
    // Use this for initialization
    void Start () {
        init_kernels();
        phoneQuat = Quaternion.identity;
		Input.gyro.enabled = true;

		rend = GetComponent<Renderer>();
		webcamtexture = new WebCamTexture();
		rend.material.mainTexture = webcamtexture;
		webcamtexture.Play();
    }

    void init_kernels()
    {
        for (int i = 0; i < blurkernelGaussian.Length; ++i)
            blurkernelGaussian[i] *= (float)1 / 16;

        for (int i = 0; i < blurkernelbox.Length; ++i)
            blurkernelbox[i] *= (float)1 / 9;

        for (int i = 0; i < blurkernelGaussian5.Length; ++i)
            blurkernelGaussian5[i] *= (float)1 / 159;
    }
	
	// Update is called once per frame
	void Update () {
        //#if UNITY_ANDROID
        phoneQuat = Input.gyro.attitude;
		Vector3 RotAngles = phoneQuat.eulerAngles;
		RotAngles = new Vector3 (RotAngles.x, RotAngles.y, RotAngles.z);
		debugText.text = phoneQuat.eulerAngles.ToString ();
		//field.transform.rotation = Quaternion.Euler (RotAngles);//Quaternion.Inverse(phoneQuat);
		//debugText.text = phoneQuat.ToString ();
        //#endif
		if (true){
		//debugText.text += "step1\n";
			setup_reduce_texture ();
			//rend.gameObject.SetActive (true);
            //float time = Time.realtimeSinceStartup;
            //blur(1);
		//debugText.text += "step1.5";
            NormalizeColor();
		//debugText.text += "step1.7";
            recognize_color_normalized(color_ref, 50f / 256f);
		//debugText.text += "step2";
            //maskImage();
            //detectFieldLines();
            //changeToGreyscale();
            //recognize_color(color_ref, 50f / 256f);
            //sobel_oprt();
            //recog_lines();
		//debugText.text += "step3";
            cleanup_mask();
            //GetComponent<OpenCVForUnitySample.HoughLines>().Recog();
            //OpenCVForUnity.
		//debugText.text += "step4";
            findFeaturePts();
		//debugText.text += "step5";
            //getrotation();
            //print(Time.realtimeSinceStartup - time);
        }
		//debug
		
		texture = Instantiate(refcamera.GetComponent<Renderer>().material.mainTexture) as Texture2D;
		for(int i = 0; i < texture.mipmapCount; ++i)
			texture.SetPixels (colors, i);
		texture.Apply ();
		refcamera.GetComponent<Renderer>().material.mainTexture = texture;
		

	}

	void setup_reduce_texture(){
        //use orig resolution
        /*
		colors = new Color[webcamtexture.width * webcamtexture.height];
		colors = webcamtexture.GetPixels ();
		width = webcamtexture.width;
		height = webcamtexture.height;
        */

        //print(webcamtexture.width);
        //print(webcamtexture.height);
		Color[] temp = new Color[webcamtexture.width * webcamtexture.height];
        temp = webcamtexture.GetPixels ();
		float mult = webcamtexture.width / width;
		height = (int)(webcamtexture.height / mult);//recalculate height here
		colors = new Color[width * height];
		for (int x = 0; x < width; x += 1) {
			for (int y = 0; y < height; y += 1) {
				float bound_x = (float)x * mult + mult;
				float bound_y = (float)y * mult + mult;
				if (bound_x >= webcamtexture.width)
					bound_x = webcamtexture.width - 1;
				if (bound_y >= webcamtexture.height)
					bound_y = webcamtexture.height - 1;

				Vector3 avgColor = Vector3.zero;
				for (int i = (int)((float)x*mult); i < bound_x; ++i) {
					for (int j = (int)((float)y*mult); j < bound_y; ++j) {
						avgColor.x += temp [webcamtexture.width * j + i].r;
						avgColor.y += temp [webcamtexture.width * j + i].g;
						avgColor.z += temp [webcamtexture.width * j + i].b;
					}
				}
				avgColor /= (bound_x - x) * (bound_y - y);
				colors [y * width + x] = new Color (avgColor.x, avgColor.y, avgColor.z);
			}
		}
		
	}

    int[] kernelx = {-1, 0, 1, -2, 0, 2, -1, 0, 1};
    int[] kernely = {-1, -2, -1, 0, 0, 0, 1, 2, 1};
    void sobel_oprt(){
        List<int[]> sobelkernels = new List<int[]>();
        sobelkernels.Add(kernelx);
        sobelkernels.Add(kernely);
        apply_convolution(sobelkernels);
    }

    int[] linekernelHori= { -1, -1, -1, 2, 2, 2, -1, -1, -1 };
    int[] linekernelVert = { -1, 2, -1, -1, 2, -1, -1, 2, -1 };
    int[] linekernelDileft = { -1, -1, 2, -1, 2, -1, 2, -1, -1};
    int[] linekernelDiright = { 2, -1, -1, -1, 2, -1, -1, -1, 2};
    void recog_lines()
    {
        List<int[]> lineDetectkernels = new List<int[]>();
        lineDetectkernels.Add(linekernelHori);
        lineDetectkernels.Add(linekernelVert);
        lineDetectkernels.Add(linekernelDileft);
        lineDetectkernels.Add(linekernelDiright);
        apply_convolution(lineDetectkernels);
    }

    float[] blurkernelGaussian = { 1, 2, 1,
                                   2, 4, 2,
                                   1, 2, 1 }; // * 1/16
    float[] blurkernelbox = { 1, 1, 1,
                              1, 1, 1,
                              1, 1, 1 }; // * 1/9

    float[] blurkernelGaussian5 = { 2, 4, 5, 4, 2,
                                    4, 9, 12, 9, 4,
                                    5, 12, 15, 12, 5,
                                    4, 9, 12, 9, 4,
                                    2, 4, 5, 4, 2 }; // * 1/159
    void blur(int time)
    {
        List<float[]> blurkernels = new List<float[]>();
        blurkernels.Add(blurkernelGaussian5);
        for(int i = 0; i < time; ++i)
            apply_convolution_float(blurkernels);
    }

    void apply_convolution_sobel(List<int[]> kernels)
    {
        if (kernels.Count == 0)
            return;

        Color[] new_colors = new Color[colors.Length];
        for (int k = 0; k < kernels.Count; ++k)
        {
            int kernelsize = kernels[k].Length;
            int dimension = ((int)(Mathf.Pow(kernelsize, 0.5f)));
            int offset = dimension / 2;
            for (int x = offset; x < width - offset; ++x)
            {
                for (int y = offset; y < height - offset; ++y)
                {
                    int countX = 0;
                    int countY = 0;
                    for (int l = kernelsize - 1; l >= 0; --l)
                    {
                        new_colors[width * y + x] += colors[width * (y - offset + countY) + x - offset + countX] * kernels[k][l];
                        countX++;
                        if (countX % (dimension) == 0)
                        {
                            countX = 0;
                            countY += 1;
                        }
                    }

                    
                }
            }
        }
    }

    void apply_convolution(List<int[]> kernels)
    {
        if (kernels.Count == 0)
            return;

        Color[] new_colors = new Color[colors.Length];
        for (int k = 0; k < kernels.Count; ++k)
        {
            int kernelsize = kernels[k].Length;
            int dimension = ((int)(Mathf.Pow(kernelsize, 0.5f)));
            int offset = dimension / 2;
            for (int x = offset; x < width - offset; ++x)
            {
                for (int y = offset; y < height - offset; ++y)
                {
                    int countX = 0;
                    int countY = 0;
                    for (int l = kernelsize - 1; l >= 0; --l)
                    {
                        new_colors[width * y + x] += colors[width * (y - offset + countY) + x - offset + countX] * kernels[k][l];
                        countX++;
                        if (countX % (dimension) == 0)
                        {
                            countX = 0;
                            countY += 1;
                        }
                    }
                }
            }
        }
    }

    void apply_convolution_float(List<float[]> kernels)
    {
        if (kernels.Count == 0)
            return;

        Color[] new_colors = new Color[colors.Length];
        for (int k = 0; k < kernels.Count; ++k){
            int kernelsize = kernels[k].Length;
            int dimension = ((int)(Mathf.Pow(kernelsize, 0.5f)));
            int offset = dimension / 2;
            for (int x = offset; x < width - offset; ++x){
                for (int y = offset; y < height - offset; ++y){
                    int countX = 0;
                    int countY = 0;
                    for (int l = kernelsize - 1; l >= 0; --l){
                        new_colors[width * y + x] += colors[width * (y-offset+countY) + x-offset+countX] * kernels[k][l];
                        countX++;
                        if (countX % (dimension) == 0)
                        {
                            countX = 0;
                            countY += 1;
                        }
                    }
                }
            }
        }
    }

    void recognize_color(Color color, float offset) {
        //colors contain the entire texture now
        for(int i = 0; i < colors.Length; ++i){
			Vector3 colordiff = new Vector3((colors[i] - color).r, (colors[i] - color).g, (colors[i] - color).b);
            if (checkColor(colordiff, offset))
				colors[i] = Color.white;
            else
				colors[i] = Color.black;
        }
    }

    //check if a color is close enough to the taarget color
    bool checkColor(Vector3 diff, float offset)
    {
        if ((Mathf.Abs(diff.x) <= default_offset) &&
            (Mathf.Abs(diff.y) <= offset) &&
            (Mathf.Abs(diff.z) <= default_offset))
            return true;

        return false;
    }

    void changeToGreyscale()
    {
        //colors contain the entire texture now
        for (int i = 0; i < colors.Length; ++i)
        {
            colors[i] = new Color(colors[i].grayscale, colors[i].grayscale, colors[i].grayscale);
            //colors[i] = new Color(0, colors[i].g, 0);
        }
    }

    void NormalizeColor()
    {		
        //colors contain the entire texture now
        for (int i = 0; i < colors.Length; ++i)
        {
            float newR = colors[i].r / (colors[i].r + colors[i].g + colors[i].b);
            float newG = colors[i].g / (colors[i].r + colors[i].g + colors[i].b);
            float newB = colors[i].b / (colors[i].r + colors[i].g + colors[i].b);
            colors[i] = new Color(newR, newG, newB);
        }
    }

    void recognize_color_normalized(Color color, float offset)
    {
        //colors contain the entire texture now
        for (int i = 0; i < colors.Length; ++i)
        {
            if ((colors[i].g > colors[i].b) && (colors[i].g > colors[i].r))
            {
                // colors[i] = Color.white;
            }
            else
                colors[i] = Color.black;
        }
    }

    void maskImage()
    {/*
        Color[] maskcolors = colors;

        //colors contain the entire texture now
        for (int i = 0; i < colors.Length; ++i)
        {
            if (maskcolors[i] == Color.black)
                colors[i] = Color.black;
        }
        */
    }

    void detectFieldLines()
    {
		/*
		Color[] maskcolors = colors;

        //colors contain the entire texture now
        for (int i = 0; i < colors.Length; ++i)
        {
            if (maskcolors[i] != Color.black)
            {//a valid region
                if ((Mathf.Abs(maskcolors[i].r - 0.333f) + Mathf.Abs(maskcolors[i].g - 0.333f) + Mathf.Abs(maskcolors[i].b - 0.333f)) < 0.05f) { }
                    //colors[i] = Color.red;
                    //colors[i] = Color.black;
            }
        }
        */
    }

    void cleanup_mask()//only keep the part with biggest area
    {
        hasExplored = new bool[colors.Length];
        idxes = new int[colors.Length];
        areas = new List<int>();
		//init
        for (int j = 0; j < colors.Length; ++j)
        {
            hasExplored[j] = false;
            idxes[j] = -1;
        }

        int maxarea = -1;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int area = getArea(width, height, x, y, 0, areas.Count);
                if (area > 0)        
                    areas.Add(area);

                if (area > maxarea)
                    maxarea = area;
            }
        }

        for (int j = 0; j < areas.Count; ++j)
        {
            if (areas[j] > maxarea)
                maxarea = areas[j];
        }

		for (int j = 0; j < colors.Length; ++j)
		{
			if (idxes[j] >= 0)
			{
				if ( (areas[idxes[j]] < (float)width*(float)height*0.02f)||((areas[idxes[j]] < maxarea) && (areas[idxes[j]] != 0)) )
				{//a valid region
					//colors[j] = preset[idxes[j] % preset.Length];
					colors[j] = Color.black;
				}
			}
		}
    }

    //math part
    //int maxx, minx, maxy, miny;
    void findSpecialCoordVal()
    {
        int maxx = -1, minx = width, maxy = -1, miny = height;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if(colors[width*y + x] != Color.black)
                {
                    if (x > maxx)
                        maxx = x;
                    if (x < minx)
                        minx = x;
                    if (y > maxy)
                        maxy = y;
                    if (y < miny)
                        miny = y;
                }
            }
        }

        List<int> maxxpts = new List<int>();
        List<int> minxpts = new List<int>();
        List<int> maxypts = new List<int>();
        List<int> minypts = new List<int>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (colors[width * y + x] != Color.black)
                {
                    if (x == maxx) {
                        colors[width * y + x] = Color.yellow;
                        maxxpts.Add(y);
                    }

                    if (x == minx) {
                        colors[width * y + x] = Color.green;
                        minxpts.Add(y);
                    }

                    if (y == maxy) {
                        colors[width * y + x] = Color.gray;
                        maxypts.Add(x);
                    }

                    if (y == miny) {
                        colors[width * y + x] = Color.blue;
                        minypts.Add(x);
                    }

                }
            }
        }
		
        getrotation(maxxpts, maxx, minxpts, minx, maxypts, maxy, minypts, miny, width, height);
    }

    void visualizeSpecialCoordVal()
    {

    }

    bool[] hasExplored;
    List<int> areas;//areas according to idx
    int[] idxes;//the idx for each pixel
    int checkPixel(int maxW, int maxH, int w, int h)//recursive
    {
        if ((w < 0) || (w >= maxW) || (h < 0) || (h >= maxH))
            return -1;

        if (colors[maxW * h + w] == Color.black)
            return -1;

        return idxes[maxW * h + w];
    }

    int getArea(int maxW, int maxH, int w, int h, int cur_area, int cur_idx){
		if ( (hasExplored[maxW * h + w]) )
			return cur_area;

		Queue<Vector2Int> queue = new Queue<Vector2Int>(); queue.Clear ();
		Vector2Int curPt = new Vector2Int(); curPt.init (w,h);
		queue.Enqueue (curPt);

		while (queue.Count > 0) {
			Vector2Int pt = queue.Dequeue ();
			hasExplored[maxW * pt.y + pt.x] = true;
			if (colors [maxW * pt.y + pt.x] != Color.black) {
				cur_area += 1;
				idxes [maxW * pt.y + pt.x] = cur_idx;
			} else
				return cur_area;

			if( (pt.x + 1 < maxW)&&(pt.x + 1 >= 0) ){
				if ((!hasExplored [maxW * pt.y + pt.x + 1]) && (colors [maxW * pt.y + pt.x + 1] != Color.black)) {
					hasExplored[maxW * pt.y + pt.x + 1] = true;
					Vector2Int temp = new Vector2Int();
					temp.init (pt.x + 1, pt.y);
					queue.Enqueue (temp);
				}
			}

			if( (pt.x - 1 < maxW)&&(pt.x - 1 >= 0) ){
				if ((!hasExplored [maxW * pt.y + pt.x - 1]) && (colors [maxW * pt.y + pt.x - 1] != Color.black)) {
					hasExplored[maxW * pt.y + pt.x - 1] = true;
					Vector2Int temp = new Vector2Int();
					temp.init (pt.x - 1, pt.y);
					queue.Enqueue (temp);
				}
			}

			if( (pt.y + 1 < maxH)&&(pt.y + 1 >= 0) ){
				if ((!hasExplored [maxW * (pt.y + 1) + pt.x]) && (colors [maxW * (pt.y + 1) + pt.x] != Color.black)) {
					hasExplored[maxW * (pt.y+1) + pt.x] = true;
					Vector2Int temp = new Vector2Int();
					temp.init (pt.x, pt.y + 1);
					queue.Enqueue (temp);
				}
			}

			if( (pt.y - 1 < maxH)&&(pt.y - 1 >= 0) ){
				if ((!hasExplored [maxW * (pt.y - 1) + pt.x]) && (colors [maxW * (pt.y - 1) + pt.x] != Color.black)) {
					hasExplored[maxW * (pt.y-1) + pt.x] = true;
					Vector2Int temp = new Vector2Int();
					temp.init (pt.x, pt.y - 1);
					queue.Enqueue (temp);
				}
			}
		}
		return cur_area;

		/*
        if ((w < 0) || (w >= maxW) || (h < 0) || (h >= maxH))
            return cur_area;

        if (hasExplored[maxW * h + w])
            return cur_area;

        hasExplored[maxW * h + w] = true;

        if (colors[maxW * h + w] == Color.black)
            return cur_area;

        //a valid pixel
        cur_area += 1;
        idxes[maxW * h + w] = cur_idx;
        cur_area = getArea(maxW, maxH, w+1, h, cur_area, cur_idx);
        cur_area = getArea(maxW, maxH, w-1, h, cur_area, cur_idx);
        cur_area = getArea(maxW, maxH, w, h+1, cur_area, cur_idx);
        cur_area = getArea(maxW, maxH, w, h-1, cur_area, cur_idx);

        return cur_area;
        */
    }

    void findFeaturePts()
    {
        findSpecialCoordVal();
    }

    void getrotation(List<int> maxxpts, int maxx, List<int> minxpts, int minx, List<int> maxypts, int maxy, List<int> minypts, int miny, int width, int height)
    {
        float pixelsizeX = GetComponent<Collider>().bounds.size.x/width;
        float pixelsizeY = GetComponent<Collider>().bounds.size.y/height;
        //print(pixelsizeX);
        //print(pixelsizeY);
        maxxpts.Sort();
        minxpts.Sort();
        maxypts.Sort();
        minypts.Sort();
		debugText.text += "before if\n";
		if ((maxxpts.Count == 0) || (minxpts.Count == 0) || (maxypts.Count == 0) || (minypts.Count == 0)) {
			print ("lossing feature points");
			return;
		}
		debugText.text += "after if\n";
        Vector2 ePt1 = new Vector2(maxx, maxxpts[maxxpts.Count-1]);
        Vector2 ePt2 = new Vector2(maxx, maxxpts[0]);
        Vector2 nPt1 = new Vector2(maxypts[maxypts.Count - 1], maxy);
        Vector2 nPt2 = new Vector2(maxypts[0], maxy);
        Vector2 sPt1 = new Vector2(minypts[minypts.Count - 1], miny);
        Vector2 sPt2 = new Vector2(minypts[0], miny);
        Vector2 wPt1 = new Vector2(minx, minxpts[minxpts.Count-1]);
        Vector2 wPt2 = new Vector2(minx, minxpts[0]);

		//first find out how many feature points we can use
        //print(ePt1);
        //print(ePt2);
        //print(sPt1);
        //print(sPt2);
		int features_remaining = 4;
		int missing_feature = -1;//-1:nothing missing, 1:sw, 2: nw, 3: ne, 4:se
		if (((wPt2 - sPt1).magnitude <= 3f) && (Mathf.Abs (wPt2.y - wPt1.y) <= 1f) && (Mathf.Abs (sPt2.y - sPt1.y) <= 1f) && ((wPt2.x - 0)<=5f) && (Mathf.Abs(sPt1.x - 0)<=5f)) {
			missing_feature = 1;
			features_remaining -= 1;
		}if (((wPt1 - nPt1).magnitude <= 3f) && (Mathf.Abs (wPt2.y - wPt1.y) <= 1f) && (Mathf.Abs (nPt2.y - nPt1.y) <= 1f) && ((wPt2.x - 0)<=5f) && (Mathf.Abs(nPt1.x - 0)<=5f)) {
			missing_feature = 2;
			features_remaining -= 1;
		}if (((nPt2 - ePt1).magnitude <= 3f) && (Mathf.Abs (nPt2.y - nPt1.y) <= 1f) && (Mathf.Abs (ePt2.y - ePt1.y) <= 1f) && (Mathf.Abs(nPt2.x - height)<=5f) && (Mathf.Abs(ePt1.x - 0)<=5f)) {
			missing_feature = 3;
			features_remaining -= 1;
		}if (((ePt2 - sPt2).magnitude <= 3f) && (Mathf.Abs (ePt2.y - ePt1.y) <= 1f) && (Mathf.Abs (sPt2.y - sPt1.y) <= 1f) && (Mathf.Abs(ePt2.x - width)<=5f) && (Mathf.Abs(sPt1.x - 0)<=5f)) {
			missing_feature = 4;
			features_remaining -= 1;
		}

		if (features_remaining < 3) {
			print ("not enough corner detected");
			return;
		}

		//calculating corners
		Vector2 pt1, pt2, ptl, ptC, ptr, ptNW, ptSW, ptSE, ptNE;//ptC is the "central" corner
		//calculate NW, SW, SE, NE pts
		pt1 = wPt1; pt2 = nPt1;
		float a1 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		float b1 = -a1 * pt1.x + pt1.y;
		pt1 = nPt2; pt2 = ePt1;
		float a2 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		float b2 = -a1 * pt1.x + pt1.y;
        ptNW.x = (b2 - b1) / (a1 - a2);// * pixelsizeX;
        ptNW.y = b1 + a1 * (b2 - b1) / (a1 - a2);// * pixelsizeY;

		pt1 = nPt2; pt2 = ePt1;
		a1 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		b1 = -a1 * pt1.x + pt1.y;
		pt1 = ePt2; pt2 = sPt2;
		a2 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		b2 = -a1 * pt1.x + pt1.y;
        ptNE.x = (b2 - b1) / (a1 - a2);// * pixelsizeX;
        ptNE.y = b1 + a1 * (b2 - b1) / (a1 - a2);// * pixelsizeY;
        
		pt1 = ePt2; pt2 = sPt2;
		a1 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		b1 = -a1 * pt1.x + pt1.y;
		pt1 = sPt1; pt2 = wPt2;
		a2 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		b2 = -a1 * pt1.x + pt1.y;
        ptSE.x = (b2 - b1) / (a1 - a2);// * pixelsizeX;
        ptSE.y = b1 + a1 * (b2 - b1) / (a1 - a2);// * pixelsizeY; 

		pt1 = wPt2; pt2 = sPt1;
		a1 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		b1 = -a1 * pt1.x + pt1.y;
		pt1 = sPt2; pt2 = ePt2;
		a2 = (pt2.y - pt1.y) / (pt2.x - pt1.x); 
		b2 = -a1 * pt1.x + pt1.y;
        ptSW.x = (b2 - b1) / (a1 - a2);// * pixelsizeX;
        ptSW.y = b1 + a1 * (b2 - b1) / (a1 - a2);// * pixelsizeY; 


		ptl = new Vector2();
		ptC = new Vector2 ();
		ptr = new Vector2 ();
		if ((missing_feature <= 0) || (missing_feature == 1)) {//sw missing
			ptl = ptNW;
			ptC = ptNE;
			ptr = ptSE;
		} else if (missing_feature == 2) {//nw
			ptl = ptNE;
			ptC = ptSE;
			ptr = ptSW;
		} else if (missing_feature == 3) {//se
			ptl = ptSW;
			ptC = ptNW;
			ptr = ptNE;
		} else {//ne
			ptl = ptSE;
			ptC = ptSW;
			ptr = ptNW;
		}
        //print(ptl);
        //print(ptC);
        //print(ptr);
        //get point in object coord
        Vector3 ptl3, ptC3, ptr3;
		ptl3 = new Vector3 ();
		ptC3 = new Vector3 ();
		ptr3 = new Vector3 ();
        if ((missing_feature <= 0) || (missing_feature == 1))
        {//sw missing
            ptl3 = new Vector3(0, 0, 0);
            ptC3 = new Vector3((float)FieldLength, 0, 0);
            ptr3 = new Vector3((float)FieldLength, -(float)FieldWidth, 0);
        }
        else if (missing_feature == 2)
        {//nw
            ptl3 = new Vector3(0, 0, 0);
            ptC3 = new Vector3(0, -(float)FieldWidth, 0);
            ptr3 = new Vector3(-(float)FieldLength, -(float)FieldWidth, 0);
        }
        else if (missing_feature == 3)
        {//se
            ptl3 = new Vector3(0, 0, 0);
            ptC3 = new Vector3(0, (float)FieldWidth, 0);
            ptr3 = new Vector3((float)FieldLength, (float)FieldWidth, 0);
        }
        else
        {//ne
            ptl3 = new Vector3(0, 0, 0);
            ptC3 = new Vector3(-(float)FieldLength, 0, 0);
            ptr3 = new Vector3(-(float)FieldLength, -(float)FieldWidth, 0);
        }
        /*
		if ((missing_feature < 0) || (missing_feature == 1)) {//sw missing
			ptl3 = new Vector3(-(float)FieldLength*0.5f,  (float)FieldWidth*0.5f, 0);
			ptC3 = new Vector3( (float)FieldLength*0.5f,  (float)FieldWidth*0.5f, 0);
			ptr3 = new Vector3( (float)FieldLength*0.5f, -(float)FieldWidth*0.5f, 0);
		} else if (missing_feature == 2) {//nw
			ptl3 = new Vector3( (float)FieldLength*0.5f,  (float)FieldWidth*0.5f, 0);
			ptC3 = new Vector3( (float)FieldLength*0.5f, -(float)FieldWidth*0.5f, 0);
			ptr3 = new Vector3(-(float)FieldLength*0.5f, -(float)FieldWidth*0.5f, 0);
		} else if (missing_feature == 3) {//se
			ptl3 = new Vector3(-(float)FieldLength*0.5f, -(float)FieldWidth*0.5f, 0);
			ptC3 = new Vector3(-(float)FieldLength*0.5f,  (float)FieldWidth*0.5f, 0);
			ptr3 = new Vector3( (float)FieldLength*0.5f,  (float)FieldWidth*0.5f, 0);
		} else {//ne
			ptl3 = new Vector3( (float)FieldLength*0.5f, -(float)FieldWidth*0.5f, 0);
			ptC3 = new Vector3(-(float)FieldLength*0.5f, -(float)FieldWidth*0.5f, 0);
			ptr3 = new Vector3(-(float)FieldLength*0.5f,  (float)FieldWidth*0.5f, 0);
		}
        */

        //solvepnp
        OpenCVForUnity.MatOfPoint3f objectPoints = new OpenCVForUnity.MatOfPoint3f ();
		OpenCVForUnity.MatOfPoint2f imagePoints = new OpenCVForUnity.MatOfPoint2f ();

		List<OpenCVForUnity.Point3> objptsList = new List<OpenCVForUnity.Point3> ();
		objptsList.Add (new OpenCVForUnity.Point3(ptl3.x, ptl3.y, ptl3.z));
		objptsList.Add (new OpenCVForUnity.Point3(ptC3.x, ptC3.y, ptC3.z));
		objptsList.Add (new OpenCVForUnity.Point3(ptr3.x, ptr3.y, ptr3.z));
		objectPoints.fromList (objptsList);

		List<OpenCVForUnity.Point> imgptsList = new List<OpenCVForUnity.Point> ();
		imgptsList.Add (new OpenCVForUnity.Point(ptl.x, ptl.y));
		imgptsList.Add (new OpenCVForUnity.Point(ptC.x, ptC.y));
		imgptsList.Add (new OpenCVForUnity.Point(ptr.x, ptr.y));
		imagePoints.fromList (imgptsList);

		Matrix4x4 p = Camera.main.projectionMatrix;
		OpenCVForUnity.Mat cameraMatrix = new OpenCVForUnity.Mat (new OpenCVForUnity.Size (4, 4), OpenCVForUnity.CvType.CV_32F);
        for(int i = 0; i < 4; ++i){
            for(int j = 0; j < 4; ++j){
                cameraMatrix.put(i,j, p[i,j]);
                //print(p[i, j]);
            }
        }

        OpenCVForUnity.Mat rvec = new OpenCVForUnity.Mat();
        OpenCVForUnity.Mat tvec = new OpenCVForUnity.Mat();
        OpenCVForUnity.MatOfDouble distCoeffs = new OpenCVForUnity.MatOfDouble();
        List<double> distCoeffsList = new List<double>();
        distCoeffsList.Add(0);
        distCoeffsList.Add(0);
        distCoeffsList.Add(0);
        distCoeffs.fromList(distCoeffsList);
        //solve!
        OpenCVForUnity.Calib3d.solvePnP(objectPoints, imagePoints, cameraMatrix, distCoeffs, rvec, tvec);

        //apply new transform
        OpenCVForUnity.Mat Rvec = new OpenCVForUnity.Mat();
        OpenCVForUnity.Mat Tvec = new OpenCVForUnity.Mat();
        OpenCVForUnity.Calib3d.Rodrigues(rvec,Rvec);

        rvec.convertTo(Rvec, OpenCVForUnity.CvType.CV_32F);
        tvec.convertTo(Tvec, OpenCVForUnity.CvType.CV_32F);

        OpenCVForUnity.Mat rotMat = new OpenCVForUnity.Mat(3, 3, OpenCVForUnity.CvType.CV_64FC1);
        OpenCVForUnity.Calib3d.Rodrigues(Rvec, rotMat);
        //string toprint = rotMat.get(0, 0)[0].ToString() + ", " + rotMat.get(1, 0)[0].ToString() + ", " + rotMat.get(2, 0)[0].ToString();
        //string toprint = Tvec.get(0, 0)[0].ToString() + ", " + Tvec.get(1, 0)[0].ToString() + ", " + Tvec.get(2, 0)[0].ToString();
        //print(toprint);
        Matrix4x4 m = new Matrix4x4();
        m.SetRow(0, new Vector4((float)rotMat.get(0, 0)[0], (float)rotMat.get(0, 1)[0], (float)rotMat.get(0, 2)[0], (float)Tvec.get(0, 0)[0]));
        m.SetRow(1, new Vector4((float)rotMat.get(1, 0)[0], (float)rotMat.get(1, 1)[0], (float)rotMat.get(1, 2)[0], (float)Tvec.get(1, 0)[0]));
        m.SetRow(2, new Vector4((float)rotMat.get(2, 0)[0], (float)rotMat.get(2, 1)[0], (float)rotMat.get(2, 2)[0], (float)Tvec.get(2, 0)[0]));
        m.SetRow(3, new Vector4(0, 0, 0, 1));

        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));

        Vector3 newPos = m.GetColumn(3);
        print(q);
        //field.transform.position = newPos;
        field.transform.rotation = q;

        /*
        //getting slope, may need only one
        float southslope = (ePt2.y - sPt1.y)* pixelsizeY / ((ePt2.x - sPt1.x)* pixelsizeX);
        Vector2 southslopeVec = new Vector2(1f, southslope).normalized;
        float northslope = (nPt1.y - wPt1.y)* pixelsizeY / ((nPt1.y - wPt1.y)* pixelsizeX);
        Vector2 startPt = (nPt1 - wPt1) * 0.5f;
        float width_in_pic = ((sPt1-startPt) - Vector2.Dot( (sPt1 - startPt),  southslopeVec )* southslopeVec).magnitude;
        float width_in_pic_half = width_in_pic * 0.5f;

		Vector3 newAxis = rotationReference.transform.up;
        float angleA;
		field.transform.rotation.ToAngleAxis(out angleA, out newAxis);
		//print (angleA);
        //changle AngleA to radius
        angleA = angleA / 180 * Mathf.PI;
		debugText.text += angleA.ToString ();
        float angleB = Mathf.Atan(width_in_pic_half/(Camera.main.transform.position - gameObject.transform.position).magnitude);
        float L = FieldWidth * 0.5f * Mathf.Sin(angleA) / Mathf.Tan(angleA) + FieldWidth * 0.5f * Mathf.Sin(angleA) / Mathf.Tan(angleB);
		//debugText.text += angleA.ToString ();
        field.transform.position = Camera.main.transform.position + Camera.main.transform.forward * L;
		print (angleA);
        /*
        float angle = Mathf.Atan(southslope);
        angle = angle / 3.1415926f * 180f;
        //print(angle);
        Vector3 deltarotation = Vector3.zero;
        deltarotation.y -= angle;
        field.transform.Rotate(deltarotation);
        */
    }
}
