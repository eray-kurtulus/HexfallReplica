using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class GameController : MonoSingleton<GameController>
{
    [SerializeField] private int boardWidth;
    [SerializeField] private int boardHeight;
    [SerializeField] private Hexagon referenceHexagon;
    [SerializeField] private JointPoint referenceJoint;
    [SerializeField] private float hexagonSize;
    [SerializeField] private float hexagonGap;
    [SerializeField] private float clickDragThreshold;
    [SerializeField] private int pointsPerHexagon;
    [SerializeField] private int scorePerBomb;
    //[SerializeField] private Button testButton;

    public Material[] colorMaterials;
    public Sprite[] bombNumberSprites;

    public Action onDoneDestroying;

    private int nextScoreForBomb;

    private int jointsWidth;
    private int jointsHeight;

    private GameObject targetJoint;
    private GameObject selectedJoint;
    private Vector3 clickPoint;
    private Vector3 releasePoint;

    private Hexagon[,] hexagons;
    private JointPoint[,] joints;
    private List<Hexagon> newlyCreatedHexagons;
    public List<Hexagon> bombs;

    private Random rand = new Random();

    // Start is called before the first frame update
    void Start()
    {
        //testButton.onClick.AddListener(ButtonTest);
        
        hexagons = new Hexagon[boardWidth,boardHeight];
        newlyCreatedHexagons = new List<Hexagon>();
        bombs = new List<Hexagon>();

        jointsWidth = boardWidth - 1;
        jointsHeight = 2 * (boardHeight - 1);
        joints = new JointPoint[jointsWidth, jointsHeight];

        nextScoreForBomb = scorePerBomb;

        InitializeHexagonBoard();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickPoint = Input.mousePosition;
            RaycastHit hitInfo;
            targetJoint = ReturnClickedObject(out hitInfo);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 releasePoint = Input.mousePosition;
            Vector3 dragVector = clickPoint - releasePoint;

            if (dragVector.magnitude > clickDragThreshold)
            {
                // the mouse was dragged more than the threshold
                if (selectedJoint != null)
                {
                    // there was a previously selected joint
                    if (dragVector.x < 0f)
                    {
                        if (releasePoint.y < this.transform.position.y)
                        {
                            // rotate right
                            RotateCommand(true);
                        }
                        else
                        {
                            // rotate left
                            RotateCommand(false);
                        }
                        
                    }
                    else
                    {
                        if (releasePoint.y < this.transform.position.y)
                        {
                            // rotate left
                            RotateCommand(false);
                        }
                        else
                        {
                            // rotate right
                            RotateCommand(true);
                        }
                    }
                }
            }
            else
            {
                // the mouse wasn't dragged more than the threshold, counts as a click
                if (targetJoint != null)
                {
                    if (selectedJoint != null)
                    {
                        // there was a previously selected joint, deselect it
                        selectedJoint.GetComponent<JointPoint>().DeselectJoint();
                    }
                    selectedJoint = targetJoint;
                    selectedJoint.GetComponent<JointPoint>().SelectJoint();
                }
            }
            
        }
    }

    void InitializeHexagonBoard()
    {
        // Helpers for Hexagon creation
        
        Hexagon newHexagon;
        Vector3 basePointer = this.transform.position;
        
        Vector3 hexagonUpOffset = Vector3.up * (hexagonSize * Mathf.Sqrt(3) / 2f + hexagonGap);
        Vector3 hexagonRightUpOffset = Quaternion.AngleAxis(60, Vector3.back) * hexagonUpOffset;
        Vector3 hexagonRightDownOffset = Quaternion.AngleAxis(120, Vector3.back) * hexagonUpOffset;
        
        // Create Hexagons
        for (int i = 0; i < boardWidth; i++)
        {
            // first, create the hexagons at the bottom
            newHexagon = Instantiate(referenceHexagon, this.transform);
            if (i % 2 == 0)
            {
                newHexagon.transform.localPosition = basePointer + hexagonRightDownOffset;
            }
            else
            {
                newHexagon.transform.localPosition = basePointer + hexagonRightUpOffset;
            }

            hexagons[i, 0] = newHexagon;
            basePointer = newHexagon.transform.localPosition;
            
            // then create hexagons above the one at the bottom
            for (int j = 1; j < boardHeight; j++)
            {
                newHexagon = Instantiate(referenceHexagon, this.transform);
                newHexagon.transform.localPosition = basePointer + hexagonUpOffset * j;
                hexagons[i, j] = newHexagon;
            }
        }

        // Helpers for JointPoint creation
        
        JointPoint newJointPoint;
        
        Vector3 hexagonSide = Vector3.right * (hexagonSize + hexagonGap) / 2f;
        basePointer = hexagons[0,0].transform.position + Quaternion.AngleAxis(-120, Vector3.back) * hexagonSide;
        Vector3 previousJointPosition;
        
        Vector3 jointUpRight = Quaternion.AngleAxis(-60, Vector3.back) * hexagonSide;
        Vector3 jointUpLeft = Quaternion.AngleAxis(-120, Vector3.back) * hexagonSide;
        
        // Create JointPoints
        for (int i = 0; i < jointsWidth; i++)
        {
            // first, create the joint at the bottom
            newJointPoint = Instantiate(referenceJoint, this.transform);
            if (i % 2 == 0)
            {
                previousJointPosition = basePointer + hexagonSide;
                newJointPoint.transform.localPosition = previousJointPosition;
                
                newJointPoint.adjointHexagons[0] = hexagons[i, 0];
                newJointPoint.adjointHexagons[1] = hexagons[i, 1]; 
                newJointPoint.adjointHexagons[2] = hexagons[i + 1, 0];
                
                newJointPoint.spriteRenderer.flipX = true;
            }
            else
            {
                previousJointPosition = basePointer +  2 * hexagonSide;
                newJointPoint.transform.localPosition = previousJointPosition;
                
                newJointPoint.adjointHexagons[0] = hexagons[i, 0];
                newJointPoint.adjointHexagons[1] = hexagons[i + 1, 1]; 
                newJointPoint.adjointHexagons[2] = hexagons[i + 1, 0];
            }
            
            newJointPoint.SetIndices(i, 0);
            joints[i, 0] = newJointPoint;
            basePointer = previousJointPosition;
            
            // then create joints above the one at the bottom
            for (int j = 1; j < jointsHeight; j++)
            {
                newJointPoint = Instantiate(referenceJoint, this.transform);
                if ((i + j) % 2 == 0)
                {
                    previousJointPosition += jointUpLeft;
                    newJointPoint.transform.localPosition = previousJointPosition;
                    
                    newJointPoint.adjointHexagons[0] = hexagons[i, j/2];
                    newJointPoint.adjointHexagons[1] = hexagons[i, j/2 + 1];
                    if (i % 2 == 0)
                    {
                        newJointPoint.adjointHexagons[2] = hexagons[i + 1, j/2];
                    }
                    else
                    {
                        newJointPoint.adjointHexagons[2] = hexagons[i + 1, j/2 + 1];
                    }
                    
                    newJointPoint.spriteRenderer.flipX = true;
                }
                else
                {
                    previousJointPosition += jointUpRight;
                    newJointPoint.transform.localPosition = previousJointPosition;
                    
                    if (i % 2 == 0)
                    {
                        newJointPoint.adjointHexagons[0] = hexagons[i, (j+1)/2];
                    }
                    else
                    {
                        newJointPoint.adjointHexagons[0] = hexagons[i, j/2];
                    }
                    newJointPoint.adjointHexagons[1] = hexagons[i + 1, j/2 + 1]; 
                    newJointPoint.adjointHexagons[2] = hexagons[i + 1, j/2];
                }
                
                newJointPoint.SetIndices(i, j);
                joints[i, j] = newJointPoint;
            }
            
        }
        
        // see if there are any scoring-points and pop them, until there aren't any
        InitialBoardCleanup();
    }
    
    GameObject ReturnClickedObject(out RaycastHit hit)
    {
        GameObject targetObject = null;
        int layerMask = LayerMask.GetMask("Joint");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit, Single.MaxValue, layerMask))
        {
            targetObject = hit.collider.gameObject;
        }
        
        return targetObject;
    }

    void InitialBoardCleanup()
    {
        if (CheckIfMarkableHexagonsExist())
        {
            MarkHexagonsForDestruction();
            DestroyMarkedHexagons(0, true);
        }
        else
        {
            if (!IsThereAnyValidMove())
            {
                print("what a luck, game is over before it started :(");
                GameOver();
            }
        }
    }
    
    bool CheckIfMarkableHexagonsExist()
    {
        for (int i = 0; i < jointsWidth; i++)
        {
            for (int j = 0; j < jointsHeight; j++)
            {
                if (joints[i, j].CheckIfMarkableHexagons())
                {
                    return true;
                }
            }
        }
        return false;
    }

    void MarkHexagonsForDestruction()
    {
        for (int i = 0; i < jointsWidth; i++)
        {
            for (int j = 0; j < jointsHeight; j++)
            {
                joints[i, j].CheckAndMarkHexagons();
            }
        }
    }

    async void DestroyMarkedHexagons(int delay, bool initialDestruction)
    {
        newlyCreatedHexagons.Clear();
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                if (hexagons[i, j].markedForDestruction)
                {
                    //hexagons[i, j].particlesOnDestroy.Play();
                    if (delay != 0)
                    {
                        await Task.Delay(delay);
                    }
                    
                    // every hexagon above falls 1 grid down
                    for (int k = j; k < boardHeight - 1; k++)
                    {
                        hexagons[i, k].SetAs(hexagons[i, k + 1]);
                        hexagons[i, k].markedForDestruction = hexagons[i, k + 1].markedForDestruction;
                    }

                    // the last hexagon is a new one
                    hexagons[i, boardHeight - 1].NewRandomHexagon();
                    newlyCreatedHexagons.Add(hexagons[i, boardHeight - 1]);

                    // re-check this index
                    j--;
                    
                }
            }
            
            if (delay != 0)
            {
                await Task.Delay(delay);
            }
        }

        if (initialDestruction)
        {
            InitialBoardCleanup();
        }
        else
        {
            onDoneDestroying();
            SuccessfulMoveRoutine(false);
        }

    }

    void RotateCommand(bool _isClockwise)
    {
        JointPoint selectedJointPoint = selectedJoint.GetComponent<JointPoint>();
        
        Rotate(selectedJointPoint, _isClockwise);
        if (CheckIfMarkableHexagonsExist())
        {
            SuccessfulMoveRoutine(true);
        }
        else
        {
            Rotate(selectedJointPoint, _isClockwise);
            if (CheckIfMarkableHexagonsExist())
            {
                SuccessfulMoveRoutine(true);
            }
            else
            {
                Rotate(selectedJointPoint, _isClockwise);
            }
        }
    }
    
    bool IsThereAnyValidMove()
    {
        // returns false if no valid moves left
        
        for (int i = 0; i < jointsWidth; i++)
        {
            for (int j = 0; j < jointsHeight; j++)
            {
                Rotate(joints[i,j], true);
                if (CheckIfMarkableHexagonsExist())
                {
                    Rotate(joints[i,j], false);
                    print(joints[i,j].horizontalIndex + " " + joints[i,j].verticalIndex);
                    return true;
                }
            
                Rotate(joints[i,j], true);
                if (CheckIfMarkableHexagonsExist())
                {
                    Rotate(joints[i,j], true);
                    print(joints[i,j].horizontalIndex + " " + joints[i,j].verticalIndex);
                    return true;
                }
                Rotate(joints[i,j], true);
            }
        }

        return false;
    }

    void Rotate(JointPoint _selectedJointPoint, bool _isClockwise)
    {
        if (_isClockwise)
        {
            _selectedJointPoint.RotateClockwise();
        }
        else
        {
            _selectedJointPoint.RotateCounterClockwise();
        }
    }

    void SuccessfulMoveRoutine(bool _tickBomb)
    {
        if (CheckIfMarkableHexagonsExist())
        {
            MarkHexagonsForDestruction();
            onDoneDestroying = () =>
            {
                int points = pointsPerHexagon * newlyCreatedHexagons.Count;
                int newScore = UIController.Instance.IncrementAndReturnScore(points);

                if (_tickBomb)
                {
                    foreach (var bomb in bombs)
                    {
                        bomb.BombTick();
                        print(bomb.movesBeforeExplosion);
                    }
                }

                if (newScore >= nextScoreForBomb)
                {
                    nextScoreForBomb += scorePerBomb;
                    int randomIndex = rand.Next(newlyCreatedHexagons.Count);
                    newlyCreatedHexagons[randomIndex].MakeBomb(true);
                }
                
                if (!IsThereAnyValidMove())
                {
                    print("no valid moves left");
                    GameOver();
                }
            };
            DestroyMarkedHexagons(100, false);
            
        }
    }

    public void ExplodeBomb()
    {
        GameOver();
    }
    
    static async void GameOver()
    {
        UIController.Instance.ShowGameOverCanvas();
        
        // wait 5 seconds and reload scene
        await Task.Delay(5000);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
