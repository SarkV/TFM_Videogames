using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEditor;

public enum Obstacles
{
    NORMAL,
    FLYING,
    NON_DODGE,
    BREAKABLE,
}

public class SimpleGameArea : MonoBehaviour
{
    public GameObject _environment;
    public GameObject _player;
    public GameObject[] _obstacles;

    public List<GameObject> _environmentList;
    List<GameObject>[] _obstaclesList;

    float speed = -14;
    int _railPosition = 6;

    public Dictionary<Obstacles, string> _obstacleType = new Dictionary<Obstacles, string>
    {
        { Obstacles.NORMAL, "obstacle" },
        { Obstacles.FLYING, "flyingObstacle" },
        { Obstacles.NON_DODGE, "nonDodgeObstacle" },
        { Obstacles.BREAKABLE, "breakableObstacle" }
    };

    Vector2 nextTime = new Vector2(1.5f, 4f);

    // Start is called before the first frame update
    void Start()
    {
        _obstaclesList = new List<GameObject>[3];
        _obstaclesList[0] = new List<GameObject>();
        _obstaclesList[1] = new List<GameObject>();
        _obstaclesList[2] = new List<GameObject>();

        Restart();
    }

    public void Restart()
    {
        CancelInvoke();
        for (int k = 0; k < 3; k++)
        {
            while (_obstaclesList[k].Count > 0)
            {
                if (_obstaclesList[k][0] != null)
                {
                    Destroy(_obstaclesList[k][0]);
                }
                _obstaclesList[k].RemoveAt(0);
            }
        }
        _environmentList[0].transform.position = new Vector3(
            _environmentList[0].transform.position.x,
            _environmentList[0].transform.position.y,
            this.transform.position.z
            );
        _environmentList[1].transform.position = new Vector3(
            _environmentList[1].transform.position.x,
            _environmentList[1].transform.position.y,
            this.transform.position.z + 64
            );
        if(_environmentList.Count == 3)
        {
            _environmentList[2].transform.position = new Vector3(
            _environmentList[2].transform.position.x,
            _environmentList[2].transform.position.y,
            this.transform.position.z + 128
            );
        }
        else
        {
            GameObject gm = Instantiate(_environment, new Vector3(_environmentList[0].transform.position.x, _environmentList[0].transform.position.y, this.transform.position.z + 128), Quaternion.identity);
            _environmentList.Add(gm);
        }
        _player.GetComponent<PlayerAgent>()._railPosition = _railPosition;
        _player.GetComponent<PlayerAgent>()._area = this;        
        _player.GetComponent<PlayerAgent>().Reset();
        Invoke("generateObstacles", 0);
    }

    void generateObstacles()
    {
        GameObject obstacle = _obstacles[Random.Range(0, _obstacles.Length)];
        int times = 0;
        if (_obstacleType[Obstacles.BREAKABLE].Equals(obstacle.tag))
        {
            times = 3;
        }
        else if (_obstacleType[Obstacles.NON_DODGE].Equals(obstacle.tag))
        {
            times = Random.Range(1,3);
        }
        else
        {
            times = Random.Range(1, 4);
        }

        List<int> positions = new List<int>() {(int) this.transform.position.x - _railPosition, (int)this.transform.position.x, (int)this.transform.position.x + _railPosition };
        List<int> indexes = new List<int>() { 0,1,2};

        while (times > 0)
        {
            int index = Random.Range(0, positions.Count);
            int pos = positions[index];
            int ind = indexes[index];
            positions.RemoveAt(index);
            indexes.RemoveAt(index);
            GameObject gm = Instantiate(obstacle, new Vector3(pos, this.transform.position.y, this.transform.position.z + 128), Quaternion.identity);
            gm.transform.rotation = obstacle.transform.rotation;
            gm.transform.position += obstacle.transform.position;
            gm.name = obstacle.name;

            _obstaclesList[ind].Add(gm);
            times--;
        }

        Invoke("generateObstacles", Random.Range(nextTime.x, nextTime.y));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(speed < 0)
        {
            speed *= -1 * Time.deltaTime;
        }
        int remove = -1;
        for (int i = 0; i < _environmentList.Count; i++)
        {
            _environmentList[i].transform.Translate(Vector3.back * speed, Space.World);
            if(_environmentList[i].transform.position.z < -63)
            {
                remove = i;
            }
        }

        if (remove > -1)
        {
            DestroyImmediate(_environmentList[remove]);
            _environmentList.RemoveAt(remove);
            GameObject gm = Instantiate(_environment, new Vector3(_environmentList[0].transform.position.x, _environmentList[0].transform.position.y, _environmentList[1].transform.position.z + 64), Quaternion.identity);
            _environmentList.Add(gm);
        }

        List<KeyValuePair<int, int>> removeList = new List<KeyValuePair<int, int>>();

        for (int k = 0; k < 3; k++)
        {
            for (int i = 0; i < _obstaclesList[k].Count; i++)
            {
                if (_obstaclesList[k][i] == null || _obstaclesList[k][i].transform.position.z < -10)
                {
                    removeList.Add(new KeyValuePair<int, int>(k, i));
                }
                else
                {
                    _obstaclesList[k][i].transform.Translate(Vector3.back * speed, Space.World);
                    if (_obstaclesList[k][i].transform.position.z < _player.transform.position.z - 5)
                    {
                        GameObject gm = _obstaclesList[k][i];
                        _obstaclesList[k].RemoveAt(i);
                        _obstaclesList[k].Add(gm);
                    }
                }
            }
        }
        if (removeList.Count > 0)
        {
            for(int i = removeList.Count - 1; i >= 0; i--)
            {
                if (_obstaclesList[removeList[i].Key][removeList[i].Value] != null)
                {
                    DestroyImmediate(_obstaclesList[removeList[i].Key][removeList[i].Value]);
                }
                _obstaclesList[removeList[i].Key].RemoveAt(removeList[i].Value);
            }
        }
    }

    public void destroyObstacle(int position)
    {
        Destroy(_obstaclesList[position][0]);
        _obstaclesList[position].RemoveAt(0);
    }

    float getObstacleType(GameObject gm)
    {
        float result = -1f;
        for (int i = 0; i < _obstacles.Length && result == -1f; i++)
        {
            if (_obstacles[i].name.Equals(gm.name))
            {
                result = i;
            }
        }
        return result / _obstacles.Length;
    }

    public float[,] getNearerObstacles()
    {
        float[,] result = new float[3,2];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 2; j++)
                result[i, j] = -1f;

        for (int i = 0; i < _obstaclesList.Length; i++)
        {
            if (_obstaclesList[i].Count > 0)
            {
                foreach(GameObject gm in _obstaclesList[i])
                {
                    if(gm != null)
                    {
                        if (gm.transform.position.z >= _player.transform.position.z-5)
                        {
                            result[i, 0] = getObstacleType(gm);
                            result[i, 1] = gm.transform.position.z;// / 128;
                            break;
                        }
                    }

                }    

            }
        }
        return result;
    }

    public int obstacleAvoided(Action currentAction, int position)
    {
        int[] pass = new int[] { 0, 0, 0 };
        if (_obstaclesList[position].Count > 0)
        {
            for(int i = 0; i < _obstaclesList.Length; i++)
            {
                if (_obstaclesList[i].Count > 0)
                {
                    GameObject gm = _obstaclesList[i][0];
                    if (gm.transform.position.z <= _player.transform.position.z + 7
                    && gm.transform.position.z >= _player.transform.position.z)
                    {
                        bool p = true;
                        if (_obstacleType[Obstacles.BREAKABLE].Equals(gm.tag))
                        {
                            p = currentAction == Action.ATTACK;
                        }
                        else if (_obstacleType[Obstacles.NON_DODGE].Equals(gm.tag))
                        {
                            p = i != position;
                        }
                        else if (_obstacleType[Obstacles.NORMAL].Equals(gm.tag))
                        {
                            p = currentAction == Action.JUMP || i != position;
                           // p = currentAction == Action.JUMP;
                        }
                        else if (_obstacleType[Obstacles.FLYING].Equals(gm.tag))
                        {
                            p = currentAction == Action.ROLL || i != position;
                          //  p = currentAction == Action.ROLL;
                        }
                        pass[i] = p ? 1 : -1;
                    }
                }                
            }
        }
        int result = 0;
        if(pass[position] != 0)
        {
            result = pass[position];
        }else if (pass[0] == 0 && pass[1] == 0 && pass[2] == 0)
        {
            result = 0;
        }
        else {
            result = 1;
        }
        return result;
    }
}
