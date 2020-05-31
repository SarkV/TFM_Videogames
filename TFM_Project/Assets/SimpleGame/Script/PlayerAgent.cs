using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public enum Action
{
    RUN,
    JUMP,
    ROLL,
    ATTACK,
    FALL,
}

public class PlayerAgent : Agent
{
    Animator _animator;
    Rigidbody _rigidbody;

    public int _railPosition = 5;

    public SimpleGameArea _area;

    static Dictionary<Action, string> _triggers = new Dictionary<Action, string>
    {
        { Action.FALL, "isInGround" },
        { Action.JUMP, "JumpTrigger" },
        { Action.ROLL, "RollTrigger" },
        { Action.ATTACK, "AttackTrigger" }
    };


    int _position = 0;

    Action _currentAction = Action.RUN;

    // Start is called before the first frame update
    public override void Initialize()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        Reset();
    }

    public override void OnEpisodeBegin()
    {
        _area.Restart();
    }

    public override void OnActionReceived(float[] vectorAction)
    {

        if (_currentAction == Action.RUN)
        {
            if (vectorAction[0] == 1)
            {
                // Move Left
                if (_position > -1)
                {
                    _position--;
                    this.transform.Translate(Vector3.left * _railPosition);
                }
                else
                {
                    AddReward(-.5f);
                }
            }
            else if (vectorAction[0] == 2)
            {
                // Move Right
                if (_position < 1)
                {
                    _position++;
                    this.transform.Translate(Vector3.right * _railPosition);
                }
                else
                {
                    AddReward(-.5f);
                }
            }
            else if (vectorAction[0] == 3)
            {
                _currentAction = Action.JUMP;
                _rigidbody.velocity = _rigidbody.velocity + (Vector3.up * 6f);
                _animator.SetBool(_triggers[Action.FALL], false);
                _animator.SetTrigger(_triggers[Action.JUMP]);
            }
            else if (vectorAction[0] == 4)
            {
                _currentAction = Action.ROLL;
                _animator.SetTrigger(_triggers[Action.ROLL]);
            }
            else if (vectorAction[0] == 5)
            {
                _currentAction = Action.ATTACK;
                _animator.SetTrigger(_triggers[Action.ATTACK]);
            }                
        }
        else if(vectorAction[0] != 0)
        {
            AddReward(-.5f);
        }

        AddReward(_area.obstacleAvoided(_currentAction, _position + 1));
    }

    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[0] = 1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[0] = 2f;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            actionsOut[0] = 3f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            actionsOut[0] = 4f;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            actionsOut[0] = 5f;
        }
        else
        {
            actionsOut[0] = 0f;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float[,] obstacles = _area.getNearerObstacles();
        sensor.AddObservation(_position);
        sensor.AddObservation(((int)_currentAction) / 3f);
        sensor.AddObservation(obstacles[0, 0]);
        sensor.AddObservation(obstacles[0, 1]);
        sensor.AddObservation(obstacles[1, 0]);
        sensor.AddObservation(obstacles[1, 1]);
        sensor.AddObservation(obstacles[2, 0]);
        sensor.AddObservation(obstacles[2, 1]);
    }

    void Failed()
    {
        EndEpisode();
        //_area.Restart();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ("ground".Equals(collision.gameObject.tag))
        {
            _animator.SetBool(_triggers[Action.FALL], true);
            setAction(Action.RUN);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_area._obstacleType[Obstacles.NORMAL].Equals(other.gameObject.tag) && _currentAction != Action.JUMP) ||
            (_area._obstacleType[Obstacles.FLYING].Equals(other.gameObject.tag) && _currentAction != Action.ROLL) ||
            _area._obstacleType[Obstacles.NON_DODGE].Equals(other.gameObject.tag))
        {
            Failed();
        }
        else if (_area._obstacleType[Obstacles.BREAKABLE].Equals(other.gameObject.tag))
        {
            if (_currentAction == Action.ATTACK)
            {
                _area.destroyObstacle(_position + 1);
            }
            else
            {
                Failed();
            }
        }
    }

    public void setAction(Action newAction)
    {
        _currentAction = newAction;
    }

    public void Reset()
    {
        _currentAction = Action.RUN;
        _animator.SetBool(_triggers[Action.FALL], true);
        if (_position == -1)
        {
            this.transform.Translate(Vector3.right * _railPosition);
        }
        else if(_position == 1)
        {
            this.transform.Translate(Vector3.left * _railPosition);
        }
        _position = 0;
    }
}
