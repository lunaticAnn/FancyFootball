using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;// for calling external program

public class fileldRecog : MonoBehaviour {

    Color color_ref = new Color(75f/255f, 111f/255f, 68f/255f);

    Renderer rend;
    Texture2D texture;
    Texture2D textureMask;//the mask used t
    Color[] colors;
    float default_offset = 20f / 256f;
    public GameObject field;
    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();
        //texture = rend.material.mainTexture as Texture2D;
        init_kernels();
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
        if (Input.GetKeyDown(KeyCode.R)){
            //float time = Time.realtimeSinceStartup;
            //blur(1);
            NormalizeColor();
            recognize_color_normalized(color_ref, 50f / 256f);
            maskImage();
            detectFieldLines();
            //changeToGreyscale();
            //recognize_color(color_ref, 50f / 256f);
            //sobel_oprt();
            //recog_lines();

            cleanup_mask();
            //GetComponent<OpenCVForUnitySample.HoughLines>().Recog();
            //OpenCVForUnity.
            findFeaturePts();
            //getrotation();
            //print(Time.realtimeSinceStartup - time);
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
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = texture.mipmapCount;
        int width = texture.width;
        int height = texture.height;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = texture.GetPixels(mip);
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
            texture.SetPixels(new_colors, mip);
            width /= 2;
            height /= 2;
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
    }

    void apply_convolution(List<int[]> kernels)
    {
        if (kernels.Count == 0)
            return;
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = texture.mipmapCount;
        int width = texture.width;
        int height = texture.height;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = texture.GetPixels(mip);
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
            texture.SetPixels(new_colors, mip);
            width /= 2;
            height /= 2;
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
    }

    void apply_convolution_float(List<float[]> kernels)
    {
        if (kernels.Count == 0)
            return;
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = texture.mipmapCount;
        int width = texture.width;
        int height = texture.height;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = texture.GetPixels(mip);
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
            texture.SetPixels(new_colors, mip);
            width /= 2;
            height /= 2;
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
    }

    void recognize_color(Color color, float offset) {
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = texture.mipmapCount;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = texture.GetPixels(mip);
            //colors contain the entire texture now
            for(int i = 0; i < colors.Length; ++i){
                Vector3 colordiff = new Vector3((colors[i] - color).r, (colors[i] - color).g, (colors[i] - color).b);
                if (checkColor(colordiff, offset))
                    colors[i] = Color.white;
                else
                    colors[i] = Color.black;
            }
            texture.SetPixels(colors, mip);
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
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
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = texture.mipmapCount;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = texture.GetPixels(mip);
            //colors contain the entire texture now
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = new Color(colors[i].grayscale, colors[i].grayscale, colors[i].grayscale);
                //colors[i] = new Color(0, colors[i].g, 0);
            }
            texture.SetPixels(colors, mip);
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
    }

    void NormalizeColor()
    {
        textureMask = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = textureMask.mipmapCount;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = textureMask.GetPixels(mip);
            //colors contain the entire texture now
            for (int i = 0; i < colors.Length; ++i)
            {
                float newR = colors[i].r / (colors[i].r + colors[i].g + colors[i].b);
                float newG = colors[i].g / (colors[i].r + colors[i].g + colors[i].b);
                float newB = colors[i].b / (colors[i].r + colors[i].g + colors[i].b);
                colors[i] = new Color(newR, newG, newB);
            }
            textureMask.SetPixels(colors, mip);
        }
        textureMask.Apply(false);
    }

    void recognize_color_normalized(Color color, float offset)
    {
        //textureMask = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = textureMask.mipmapCount;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = textureMask.GetPixels(mip);
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
            textureMask.SetPixels(colors, mip);
        }
        textureMask.Apply(false);
    }

    void maskImage()
    {
        Color[] maskcolors;
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = texture.mipmapCount;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = texture.GetPixels(mip);
            maskcolors = textureMask.GetPixels(mip);
            //colors contain the entire texture now
            for (int i = 0; i < colors.Length; ++i)
            {
                if (maskcolors[i] == Color.black)
                    colors[i] = Color.black;
            }
            texture.SetPixels(colors, mip);
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
    }

    void detectFieldLines()
    {
        Color[] maskcolors;
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int mipCount = texture.mipmapCount;
        for (int mip = 0; mip < mipCount; ++mip)
        {
            colors = texture.GetPixels(mip);
            maskcolors = textureMask.GetPixels(mip);
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
            texture.SetPixels(colors, mip);
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
    }

    void cleanup_mask()//only keep the part with biggest area
    {
        int mipCount = textureMask.mipmapCount;
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int width = texture.width;
        int height = texture.height;
        for (int i = 0; i < mipCount; ++i){
            if (Mathf.Max(width, height) > 256)
                continue;

            colors = texture.GetPixels(i);
            hasExplored = new bool[colors.Length];
            idxes = new int[colors.Length];
            areas = new List<int>();
            for (int j = 0; j < colors.Length; ++j)
            {//init hasExplored array
                hasExplored[j] = false;
                idxes[j] = -1;
            }

            int maxarea = -1;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {/*
                    int newidx = checkPixel(width, height, x - 1, y);
                    if (newidx != -1)
                    {
                        idxes[width * y + x] = newidx;
                        areas[newidx] += 1;
                    }
                    else
                    {
                        newidx = checkPixel(width, height, x, y - 1);
                        if (newidx != -1)
                        {
                            idxes[width * y + x] = newidx;
                            areas[newidx] += 1;
                        }
                    }

                    if ((colors[width * y + x] != Color.black) && (newidx == -1))
                    {
                        idxes[width * y + x] = areas.Count;// a new one
                        areas.Add(1);
                    }
                    */


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

            //int colorcount = 0;
            Color[] preset = {Color.blue, Color.yellow, Color.white, Color.magenta, Color.cyan, Color.green, Color.gray};
            for (int j = 0; j < colors.Length; ++j)
            {
                if (idxes[j] >= 0)
                {
                    if ((areas[idxes[j]] < maxarea) && (areas[idxes[j]] != 0))
                    {//a valid region
                        //colors[j] = preset[idxes[j] % preset.Length];
                        colors[j] = Color.black;
                    }
                }
            }

            texture.SetPixels(colors, i);
            width /= 2;
            height /= 2;
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
    }

    //math part
    //int maxx, minx, maxy, miny;
    void findSpecialCoordVal()
    {
        int mipCount = textureMask.mipmapCount;
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int width = texture.width;
        int height = texture.height;
        for (int i = 0; i < mipCount; ++i)
        {
            if (Mathf.Max(width, height) > 256)
                continue;
            //print(width);
            colors = texture.GetPixels(i);

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

            if(i == 0)
                getrotation(maxxpts, maxx, minxpts, minx, maxypts, maxy, minypts, miny, width, height);
            texture.SetPixels(colors, i);
            width /= 2;
            height /= 2;
        }
        texture.Apply(false);
        rend.material.mainTexture = texture;
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

    int getArea(int maxW, int maxH, int w, int h, int cur_area, int cur_idx)//recursive
    {
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
    }

    void findFeaturePts()
    {
        findSpecialCoordVal();
        
        int mipCount = textureMask.mipmapCount;
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        int width = texture.width;
        int height = texture.height;
        for (int i = 0; i < mipCount; ++i)
        {
            if (Mathf.Max(width, height) > 256)
                continue;

            colors = texture.GetPixels(i);
        }
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
        if ((maxxpts.Count == 0) || (minxpts.Count == 0) || (maxypts.Count == 0) || (minypts.Count == 0))
            return;
        Vector2 ePt1 = new Vector2(maxx, maxxpts[maxxpts.Count-1]);
        Vector2 ePt2 = new Vector2(maxx, maxxpts[0]);
        Vector2 nPt1 = new Vector2(maxypts[maxypts.Count - 1], maxy);
        Vector2 nPt2 = new Vector2(maxypts[0], maxy);
        Vector2 sPt1 = new Vector2(minypts[minypts.Count - 1], miny);
        Vector2 sPt2 = new Vector2(minypts[0], miny);
        Vector2 wPt1 = new Vector2(minx, minxpts[minxpts.Count-1]);
        Vector2 wPt2 = new Vector2(minx, minxpts[0]);

        //print(ePt1);
        //print(ePt2);
        //print(sPt1);
        //print(sPt2);
        float southslope = (ePt2.y - sPt1.y)* pixelsizeY / ((ePt2.x - sPt1.x)* pixelsizeX);
        float angle = Mathf.Atan(southslope);
        angle = angle / 3.1415926f * 180f;
        //print(angle);
        Vector3 deltarotation = Vector3.zero;
        deltarotation.y -= angle;
        field.transform.Rotate(deltarotation);
    }
}
