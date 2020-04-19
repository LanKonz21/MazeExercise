using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MazeSceneManager : MonoBehaviour
{
    #region Public Variables
    public static MazeSceneManager instance;

    public bool isDebug = false;

    public Camera cameraMain;

    public GameObject floorW, floorB, objMouse, objGoal;

    [Range(1, 10)]
    public int width, height;

    public float floatMoveTime = 1;
    #endregion

    #region Private Variables
    [Header("Debug觀察區")]   

    [SerializeField]
    Vector3 vector3MouseBack = Vector3.one;

    [SerializeField]
    bool isFind = false, isWait = false;

    [SerializeField]
    int intX, intY;
   
    Coordinate[,] arrayCoordinateFloor;

    Transform transformMouse;

    Quaternion quaternionUp = new Quaternion(0, 0, 0, 1),
               quaternionRight = new Quaternion(0, 0.7071068f, 0, 0.7071068f),
               quaternionDown = new Quaternion(0, 1, 0, 0),
               quaternionLeft = new Quaternion(0, -0.7071068f, 0, 0.7071068f);

    Stack<Vector2> stackVector2 = new Stack<Vector2>();

    WaitForSeconds waitSecMoveTime,
        waitSecWidth, waitSecHeight,
        waitSecOne = new WaitForSeconds(1),
        waitSecTwo = new WaitForSeconds(2),
        waitSecFive = new WaitForSeconds(5);
    #endregion

    #region Public Method
    public void SetFind(bool value)
    {
        isFind = value;
    }
    #endregion

    #region Private Method
    void Awake()
    {
        InstanceSceneManager();

        transformMouse = objMouse.transform;
        waitSecMoveTime = new WaitForSeconds(floatMoveTime);
        waitSecWidth = new WaitForSeconds(width);
        waitSecHeight = new WaitForSeconds(height);
        arrayCoordinateFloor = new Coordinate[width , height];        
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
    } 

    void Start()
    {
        StartCoroutine(MainOperationSequence());
    }

    void InstanceSceneManager()
    {
        if (instance != null) { DestroyImmediate(this); }
        else { instance = this; }
    }

    IEnumerator MainOperationSequence()
    {
        yield return CreateFloor();
        yield return waitSecOne;
        yield return CreatStart();
        yield return waitSecTwo;
        yield return CreatGoal();
        yield return waitSecOne;
        yield return FindGold();
        yield return FindEnd();
    }

    IEnumerator CreateFloor()
    {
        int EdgeR = width - 1, 
            EdgeU = height - 1;
        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                arrayCoordinateFloor[i,j].SetCoordinate(i, j);
                arrayCoordinateFloor[i, j].isLeft = i == 0 ? true : false;
                arrayCoordinateFloor[i, j].isRight = i == EdgeR ? true : false;
                arrayCoordinateFloor[i, j].isDown = j == 0 ? true : false;
                arrayCoordinateFloor[i, j].isUp = j == EdgeU ? true : false;
                if ((i + j) % 2 == 0) { arrayCoordinateFloor[i,j].Floor = Instantiate(floorB); continue; }
                arrayCoordinateFloor[i,j].Floor = Instantiate(floorW);
            }
        }

        foreach (Coordinate cg in arrayCoordinateFloor) { cg.Floor.transform.DOMoveZ(cg.y, cg.y, false).SetEase(Ease.OutQuart); } //height可以試試看自己座標
        yield return waitSecHeight;

        foreach (Coordinate cg in arrayCoordinateFloor) { cg.Floor.transform.DOMoveX(cg.x, cg.x, false).SetEase(Ease.OutQuart); }
        yield return waitSecWidth;

        Vector3 rot = new Vector3(0,90,0);
        foreach(Coordinate cg in arrayCoordinateFloor)
        {
            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(cg.Floor.transform.DOScale(0.1f, 1).SetEase(Ease.InQuad)).Join(cg.Floor.transform.DORotate(rot, 1,RotateMode.FastBeyond360));
        }
    }

    IEnumerator CreatStart()
    {      
        Coordinate c = arrayCoordinateFloor[UnityEngine.Random.Range(0, width),UnityEngine.Random.Range(0, height)];
        objMouse.transform.position = new Vector3(c.x,1,c.y);
        stackVector2.Push(new Vector2(c.x,c.y));
        cameraMain.cullingMask |= (1 << 10);
        yield return null;
    }

    IEnumerator CreatGoal()
    {
        Coordinate c;
        Vector2 g;
        while (true)
        {
            c = arrayCoordinateFloor[UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height)];
            g = new Vector2(c.x,c.y);
            if (!stackVector2.Contains(g))
            {
                objGoal.transform.position = new Vector3(c.x,0,c.y);
                cameraMain.cullingMask |= (1 << 11);
                break;
            }
        }
        yield return null;
    }

    IEnumerator FindGold()
    {
        int intPass = 0;
        Vector2 vector2Temp = new Vector2();
        isFind = true;
        while (isFind)
        {
            isWait = false;
            vector2Temp = stackVector2.Peek();
            intX = (int)vector2Temp.x;
            intY = (int)vector2Temp.y;
            switch (UnityEngine.Random.Range(0, 10000) % 4)
            {
                case 0: //上
                    intPass = intY + 1;
                    if (arrayCoordinateFloor[intX, intY].DeadEnd()) {  MoveBack(intX, intY); break; }                  
                    if (arrayCoordinateFloor[intX, intY].isUp) { break; }
                    if (isDebug) { Debug.Log(string.Format("1.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    arrayCoordinateFloor[intX,intY].isUp = true;
                    if (isDebug) { Debug.Log(string.Format("2.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    vector2Temp.y = intPass;
                    stackVector2.Push(vector2Temp);
                    arrayCoordinateFloor[intX, intPass].isDown = true;
                    transformMouse.rotation = quaternionUp;
                    isWait = true;
                    transformMouse.DOMoveZ(intPass, floatMoveTime);
                    break;
                case 1: //右
                    intPass = intX + 1;
                    if (arrayCoordinateFloor[intX, intY].DeadEnd()) { MoveBack(intX, intY); break; }
                    if (arrayCoordinateFloor[intX, intY].isRight) { break; }
                    if (isDebug) { Debug.Log(string.Format("1.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    arrayCoordinateFloor[intX, intY].isRight = true;
                    if (isDebug) { Debug.Log(string.Format("2.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    vector2Temp.x = intPass;
                    stackVector2.Push(vector2Temp);
                    arrayCoordinateFloor[intPass, intY].isLeft = true;
                    transformMouse.rotation = quaternionRight;
                    isWait = true;
                    transformMouse.DOMoveX(intPass, floatMoveTime);
                    break;
                case 2: //下
                    intPass = intY - 1;
                    if (arrayCoordinateFloor[intX, intY].DeadEnd()) { MoveBack(intX, intY); break; }
                    if (arrayCoordinateFloor[intX, intY].isDown) { break; }
                    if (isDebug) { Debug.Log(string.Format("1.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    arrayCoordinateFloor[intX, intY].isDown = true;
                    if (isDebug) { Debug.Log(string.Format("2.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    vector2Temp.y = intPass;
                    stackVector2.Push(vector2Temp);
                    arrayCoordinateFloor[intX, intPass].isUp = true;
                    transformMouse.rotation = quaternionDown;
                    isWait = true;
                    transformMouse.DOMoveZ(intPass, floatMoveTime);
                    break;
                case 3: //左
                    intPass = intX - 1;
                    if (arrayCoordinateFloor[intX, intY].DeadEnd()) { MoveBack(intX, intY); break; }
                    if (arrayCoordinateFloor[intX, intY].isLeft) { break; }
                    if (isDebug) { Debug.Log(string.Format("1.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    arrayCoordinateFloor[intX, intY].isLeft = true;
                    if (isDebug) { Debug.Log(string.Format("2.Mouse[ {0}, {1} ] /↑{2} /↓{3} /→{4} /←{5}", arrayCoordinateFloor[intX, intY].x, arrayCoordinateFloor[intX, intY].y, arrayCoordinateFloor[intX, intY].isUp, arrayCoordinateFloor[intX, intY].isDown, arrayCoordinateFloor[intX, intY].isRight, arrayCoordinateFloor[intX, intY].isLeft)); }
                    vector2Temp.x = intPass;
                    stackVector2.Push(vector2Temp);
                    arrayCoordinateFloor[intPass, intY].isRight = true;
                    transformMouse.rotation = quaternionLeft;
                    isWait = true;
                    transformMouse.DOMoveX(intPass, floatMoveTime);
                    break;
                default:
                    Debug.LogError("超出範圍");
                    break;

            }
            if (!isWait) { continue; }
            yield return waitSecMoveTime;
        }
    }

    IEnumerator FindEnd()
    {
        if (isDebug) { Debug.Log("目標已找到，五秒後重新開始"); }
        yield return waitSecFive;
        SceneManager.LoadScene(0);
    }

    void MoveBack(int intX,int intY)
    {
        stackVector2.Pop();
        vector3MouseBack.x = stackVector2.Peek().x;
        vector3MouseBack.z = stackVector2.Peek().y;
        if (intX > vector3MouseBack.x) { transformMouse.rotation = quaternionLeft; }
        else if (intX < vector3MouseBack.x) { transformMouse.rotation = quaternionRight; }
        if (intY > vector3MouseBack.z) { transformMouse.rotation = quaternionDown; }
        else if (intY < vector3MouseBack.z) { transformMouse.rotation = quaternionUp; }
        transformMouse.DOMove(vector3MouseBack, floatMoveTime).SetEase(Ease.OutQuad);
        isWait = true;
    }
    #endregion
}

[Serializable]
public struct Coordinate
{
    public int x,y;
    public GameObject Floor;
    public bool isUp, isDown, isRight, isLeft;
    public void SetCoordinate(int width, int height)
    {
        x = width;
        y = height;
    }

    public bool DeadEnd()
    {
        return (isUp && isDown && isRight && isLeft) ? true : false;
    }
}
