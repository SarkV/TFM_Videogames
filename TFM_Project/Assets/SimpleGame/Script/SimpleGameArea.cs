using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEditor;

public class SimpleGameArea : MonoBehaviour
{
    public GameObject _environment;
    public GameObject _player;
    public GameObject[] _obstacles;

    public List<GameObject> _environmentList;
    List<GameObject> _obstaclesList;

    float speed = -14;
    int _railPosition = 6;

    Vector2 nextTime = new Vector2(2f, 6f);

    // Start is called before the first frame update
    void Start()
    {
        _obstaclesList = new List<GameObject>();
        Restart();
    }

    public void Restart()
    {
        CancelInvoke();
        while(_obstaclesList.Count > 0)
        {
            if(_obstaclesList[0] != null)
            {
                Destroy(_obstaclesList[0]);
                _obstaclesList.RemoveAt(0);
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
        if ("breakableObstacle".Equals(obstacle.tag))
        {
            times = 3;
        }
        else if ("nonDodgeObstacle".Equals(obstacle.tag))
        {
            times = Random.Range(1,3);
        }
        else
        {
            times = Random.Range(1, 4);
        }

        List<int> positions = new List<int>() {(int) this.transform.position.x - _railPosition, (int)this.transform.position.x, (int)this.transform.position.x + _railPosition };
        while (times > 0)
        {
            int index = Random.Range(0, positions.Count);
            int pos = positions[index];
            positions.RemoveAt(index);
            GameObject gm = Instantiate(obstacle, new Vector3(pos, this.transform.position.y, this.transform.position.z + 128), Quaternion.identity);
            gm.transform.rotation = obstacle.transform.rotation;
            gm.transform.position += obstacle.transform.position;
            _obstaclesList.Add(gm);
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

        List<int> removeList = new List<int>();

        for (int i = 0; i < _obstaclesList.Count; i++)
        {            
            if (_obstaclesList[i] == null || _obstaclesList[i].transform.position.z < -63)
            {
                removeList.Add(i);
            }
            else
            {
                _obstaclesList[i].transform.Translate(Vector3.back * speed, Space.World);
            }
        }
        if (removeList.Count > 0)
        {
            for(int i = removeList.Count - 1; i > 0; i--)
            {
                if (_obstaclesList[removeList[i]] != null)
                {
                    DestroyImmediate(_obstaclesList[removeList[i]]);
                }
                _obstaclesList.RemoveAt(removeList[i]);
            }
        }
    }

    float getObstacleType(GameObject gm)
    {
        float result = -1f;
        for (int i = 0; i < _obstacles.Length && result == -1f; i++)
        {
            if (PrefabUtility.GetPrefabAssetType(gm).Equals(PrefabUtility.GetPrefabAssetType(_obstacles[i])))
            {
                result = i;
            }
        }
        return result;
    }

    public float[,] getNearerObstacles()
    {
        float[,] result = new float[3,2];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 2; j++)
                result[i, j] = -1f;

        foreach (GameObject gm in _obstacles)
        {
            if (gm.transform.position.z > _player.transform.position.z)
            {
                if (gm.transform.position.x == this.transform.position.x - _railPosition)
                {
                    if (result[0, 0] == -1 || result[0, 1] > gm.transform.position.z)
                    {
                        result[0, 0] = getObstacleType(gm);
                        result[0, 1] = gm.transform.position.x;
                    }
                }
                else if (gm.transform.position.x == this.transform.position.x)
                {
                    if (result[1, 0] == -1 || result[1, 1] > gm.transform.position.z)
                    {
                        result[1, 0] = getObstacleType(gm);
                        result[1, 1] = gm.transform.position.x;
                    }
                }
                else if (gm.transform.position.x == this.transform.position.x + _railPosition)
                {
                    if (result[2, 0] == -1 || result[1, 1] > gm.transform.position.z)
                    {
                        result[2, 0] = getObstacleType(gm);
                        result[2, 1] = gm.transform.position.x;
                    }
                }

            }
        }
        if(result[0,0] != -1)
        {
            result[0, 0] = result[0, 0] / _obstacles.Length;
            result[0, 1] = result[0, 1] / 128;
        }
        if (result[1, 0] != -1)
        {
            result[1, 0] = result[1, 0] / _obstacles.Length;
            result[1, 1] = result[1, 1] / 128;
        }
        if (result[2, 0] != -1)
        {
            result[2, 0] = result[2, 0] / _obstacles.Length;
            result[2, 1] = result[2, 1] / 128;
        }
        return result;
    }
}
