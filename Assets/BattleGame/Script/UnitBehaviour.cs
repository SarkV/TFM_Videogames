using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    UP = -1,
    DOWN = 1,
    LEFT = -2,
    RIGHT = 2
}

public enum Monster
{
    SCISSORS,
    ROCK,
    PAPER,
    LIZARD,
    SPOKE
}

public class UnitBehaviour : MonoBehaviour
{
    static float _speed = 1f;

    GameObject[] _currentPath = new GameObject[8];
    GameObject[] _auxiliarPath = new GameObject[8];

    public Monster _type;

    public string[] _beatEnemies = new string[2];

    Animator _animator;

    int _nextPath = 0;
    public AgentType _owner;
    public Direction _facing = Direction.DOWN;

    BattleGameArea _area;
    GameObject _directionArrow;


    public void setDirection(GameObject plane, GameObject directionArrow, AgentType owner)
    {
        _animator = GetComponent<Animator>();
        _owner = owner;
        _directionArrow = directionArrow;

        switch (owner)
        {
            case AgentType.PLAYER:
                {
                    _currentPath[0] = plane.transform.Find("Initial1").gameObject;
                    _currentPath[1] = plane.transform.Find("Path1_1").gameObject;
                    _currentPath[2] = plane.transform.Find("Path1_2").gameObject;
                    _currentPath[3] = plane.transform.Find("Path1_21").gameObject;
                    _currentPath[4] = plane.transform.Find("Path2_21").gameObject;
                    _currentPath[5] = plane.transform.Find("Path2_2").gameObject;
                    _currentPath[6] = plane.transform.Find("Path2_1").gameObject;
                    _currentPath[7] = plane.transform.Find("Initial2").gameObject;

                    _auxiliarPath[0] = plane.transform.Find("Initial1").gameObject;
                    _auxiliarPath[1] = plane.transform.Find("Path1_1").gameObject;
                    _auxiliarPath[2] = plane.transform.Find("Path1_2").gameObject;
                    _auxiliarPath[3] = plane.transform.Find("Path1_22").gameObject;
                    _auxiliarPath[4] = plane.transform.Find("Path2_22").gameObject;
                    _auxiliarPath[5] = plane.transform.Find("Path2_2").gameObject;
                    _auxiliarPath[6] = plane.transform.Find("Path2_1").gameObject;
                    _auxiliarPath[7] = plane.transform.Find("Initial2").gameObject;
                    break;
                }
            case AgentType.ENEMY:
                {
                    _currentPath[0] = plane.transform.Find("Initial2").gameObject;
                    _currentPath[1] = plane.transform.Find("Path2_1").gameObject;
                    _currentPath[2] = plane.transform.Find("Path2_2").gameObject;
                    _currentPath[3] = plane.transform.Find("Path2_21").gameObject;
                    _currentPath[4] = plane.transform.Find("Path1_21").gameObject;
                    _currentPath[5] = plane.transform.Find("Path1_2").gameObject;
                    _currentPath[6] = plane.transform.Find("Path1_1").gameObject;
                    _currentPath[7] = plane.transform.Find("Initial1").gameObject;

                    _auxiliarPath[0] = plane.transform.Find("Initial2").gameObject;
                    _auxiliarPath[1] = plane.transform.Find("Path2_1").gameObject;
                    _auxiliarPath[2] = plane.transform.Find("Path2_2").gameObject;
                    _auxiliarPath[3] = plane.transform.Find("Path2_22").gameObject;
                    _auxiliarPath[4] = plane.transform.Find("Path1_22").gameObject;
                    _auxiliarPath[5] = plane.transform.Find("Path1_2").gameObject;
                    _auxiliarPath[6] = plane.transform.Find("Path1_1").gameObject;
                    _auxiliarPath[7] = plane.transform.Find("Initial1").gameObject;
                    break;
                }
        }
        _animator.SetInteger("direction", (int) _facing);
        _area = plane.GetComponent<BattleGameArea>();

        this.transform.position = _currentPath[0].transform.position;
        _nextPath++;
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentPath != null)
        {
            Vector3 currentPathPos = _currentPath[_nextPath].transform.position;
            if (_nextPath == _currentPath.Length - 1)
            {
                if (currentPathPos.x == this.transform.position.x && currentPathPos.z == this.transform.position.z)
                {
                    Destroy(this.gameObject);
                    _area.hitBase(_owner);
                }
            }else if (currentPathPos.x == this.transform.position.x && currentPathPos.z == this.transform.position.z)
            {
                if (_nextPath == 2)
                {
                    if (Mathf.Abs(_directionArrow.transform.localRotation.z) > 0.1f)
                    {
                        _currentPath = _auxiliarPath;
                    }
                }
                _nextPath++;


                Vector3 nextPathPos = _currentPath[_nextPath].transform.position;
                if (Mathf.Abs(currentPathPos.x - nextPathPos.x) > Mathf.Abs(currentPathPos.z - nextPathPos.z))
                {
                    if (currentPathPos.x > nextPathPos.x) _facing = Direction.LEFT;
                    else if (currentPathPos.x < nextPathPos.x) _facing = Direction.RIGHT;
                }
                else
                {
                    if (currentPathPos.z > nextPathPos.z) _facing = Direction.DOWN;
                    else if (currentPathPos.z < nextPathPos.z) _facing = Direction.UP;
                }
                _animator.SetInteger("direction", (int)_facing);
                currentPathPos = nextPathPos;
            }
            if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
            {
                float step = _speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, currentPathPos, step);
            }
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<UnitBehaviour>()._owner != this._owner && !collision.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("Dead"))
        {
            if (_beatEnemies[0].Equals(collision.gameObject.name) || _beatEnemies[1].Equals(collision.gameObject.name))
            {
                _animator.SetTrigger("attack");
            }
            else
            {
                _animator.SetBool("isDead", true);
            }
        }
    }

    public void die()
    {
        _area.unitKilled(_owner, gameObject);
        Destroy(this.gameObject);
    }
}
